namespace OCEL

open System
open LiteDB
open OCEL.Types

module OcelLiteDB =
    
    (* --- PRIVATE MEMBERS --- *)

    let private configureBsonmapper() =
        // Custom DateTimeOffset handler (https://github.com/mbdavid/LiteDB/issues/1686#issuecomment-642215514)
        BsonMapper.Global.RegisterType<DateTimeOffset>(
            fun dto ->
                let doc = BsonDocument()
                doc["DateTime"] <- dto.DateTime.Ticks
                doc["Offset"] <- dto.Offset.Ticks
                doc
            ,
            fun doc -> DateTimeOffset(doc["DateTime"].AsInt64, TimeSpan(doc["Offset"].AsInt64))
        )

        // Custom OcelValue handler to reduce size of stored data (otherwise excludes internal properties like IsOcelString, IsOcelTimestamp, ...)
        BsonMapper.Global.RegisterType<OcelValue>(
            fun value ->
                let doc = BsonDocument()
                match value with
                | OcelString s ->
                    doc["type"] <- nameof(OcelString)
                    doc["val"] <- s
                | OcelTimestamp t -> 
                    doc["type"] <- nameof(OcelTimestamp)
                    doc["val"] <- BsonMapper.Global.ToDocument t
                | OcelInteger i -> 
                    doc["type"] <- nameof(OcelInteger)
                    doc["val"] <- i
                | OcelFloat f -> 
                    doc["type"] <- nameof(OcelFloat)
                    doc["val"] <- f
                | OcelBoolean b -> 
                    doc["type"] <- nameof(OcelBoolean)
                    doc["val"] <- b
                doc
            ,
            fun doc ->
                match doc["type"].AsString, doc["val"] with
                | nameof(OcelString), v -> OcelString v.AsString
                | nameof(OcelTimestamp), v -> BsonMapper.Global.Deserialize<DateTimeOffset> v |> OcelTimestamp
                | nameof(OcelInteger), v -> OcelInteger v.AsInt64
                | nameof(OcelFloat), v -> OcelFloat v.AsDouble
                | nameof(OcelBoolean), v -> OcelBoolean v.AsBoolean
                | _ -> raise (ArgumentOutOfRangeException "type")
        )

        // Custom OcelAttributes handler
        BsonMapper.Global.RegisterType<OcelAttributes>(
            fun map ->
                let doc = BsonDocument()
                map |> Map.iter (fun id value -> doc[id] <- BsonMapper.Global.ToDocument value)
                doc
            ,
            fun doc ->
                match doc with
                | :? BsonDocument as doc -> 
                    doc 
                    |> Seq.map (fun kv -> kv.Key, kv.Value |> BsonMapper.Global.Deserialize<OcelValue>)
                    |> Map.ofSeq
                | _ -> Map.empty
        )

        // Custom Events handler
        BsonMapper.Global.RegisterType<string * OcelEvent>(
            fun (id, event) ->
                let doc = BsonDocument()
                doc["_id"] <- id
                doc["activity"] <- event.Activity
                doc["timestamp"] <- BsonMapper.Global.ToDocument event.Timestamp
                doc["omap"] <- event.OMap |> Seq.map BsonValue |> BsonArray
                doc["vmap"] <- BsonMapper.Global.ToDocument event.VMap
                doc
            ,
            fun doc -> doc["_id"].AsString, { 
                Activity = doc["activity"].AsString
                Timestamp = BsonMapper.Global.Deserialize<DateTimeOffset> doc["timestamp"]
                OMap = doc["omap"].AsArray |> Seq.map (fun o -> o.AsString)
                VMap = BsonMapper.Global.Deserialize<OcelAttributes> doc["vmap"]
            })

        // Custom Objects handler
        BsonMapper.Global.RegisterType<string * OcelObject>(
            fun (id, obj) ->
                let doc = BsonDocument()
                doc["_id"] <- id
                doc["type"] <- obj.Type
                doc["ovmap"] <- BsonMapper.Global.ToDocument obj.OvMap
                doc
            ,
            fun doc -> doc["_id"].AsString, { 
                Type = doc["type"].AsString
                OvMap = BsonMapper.Global.Deserialize<OcelAttributes> doc["ovmap"]
            }
        )

        // Custom GlobalAttributes handler
        BsonMapper.Global.RegisterType<string * OcelValue>(
            fun (key, value) ->
                let doc = BsonDocument()
                doc["_id"] <- key
                doc["val"] <- BsonMapper.Global.ToDocument value
                doc
            ,
            fun doc -> doc["_id"].AsString, doc["val"] |> BsonMapper.Global.Deserialize<OcelValue>
        )
    
    (* --- PUBLIC MEMBERS --- *)
    
    /// <inheritdoc />
    let validateWithErrorMessage (db: LiteDatabase) =
        let mutable errors = []
        if db.CollectionExists "events" |> not then errors <- "Collection 'events' does not exist." :: errors
        if db.CollectionExists "objects" |> not then errors <- "Collection 'objects' does not exist." :: errors
        if db.CollectionExists "global_attributes" |> not then errors <- "Collection 'global_attributes' does not exist." :: errors
        errors.IsEmpty, errors |> List.rev :> seq<_>
        
    /// <inheritdoc />
    let validate (db: LiteDatabase) =
        db |> validateWithErrorMessage |> fst
    
    /// <inheritdoc />
    let deserialize (db: LiteDatabase)  =
        configureBsonmapper()
        {
            Events = db.GetCollection<string * OcelEvent>("events").FindAll() |> Map.ofSeq
            Objects = db.GetCollection<string * OcelObject>("objects").FindAll() |> Map.ofSeq
            GlobalAttributes = db.GetCollection<string * OcelValue>("global_attributes").FindAll() |> Map.ofSeq
        }
    
    /// <inheritdoc />
    let serialize (db: LiteDatabase) (log: OcelLog) =
        let insertConditional (cond: ILiteCollection<'a> -> 'a -> bool) coll objs =
            objs |> Seq.filter (cond coll) |> coll.InsertBulk

        let skipDuplicateId (coll: ILiteCollection<'a>) (obj: string * 'b) = 
            fst obj |> coll.FindById :> obj = null

        configureBsonmapper()

        let eventsColl = db.GetCollection<string * OcelEvent>("events")
        (eventsColl, log.Events |> Map.toSeq) ||> insertConditional skipDuplicateId |> ignore
        eventsColl.EnsureIndex("event_activities", "$.events.activity", false) |> ignore
        eventsColl.EnsureIndex("event_objects", "$.events.omap", false) |> ignore
        
        let objectsColl = db.GetCollection<string * OcelObject>("objects")
        (objectsColl, log.Objects |> Map.toSeq) ||> insertConditional skipDuplicateId |> ignore
        objectsColl.EnsureIndex("object_types", "$.objects.type", false) |> ignore

        let globalAttrsColl = db.GetCollection<string * OcelValue>("global_attributes")
        (globalAttrsColl, log.GlobalAttributes |> Map.toSeq) ||> insertConditional skipDuplicateId |> ignore