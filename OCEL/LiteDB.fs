namespace OCEL

open System
open LiteDB
open OCEL.Types

module LiteDB =

    (* --- PRIVATE MEMBERS --- *)

    
    (* --- PUBLIC MEMBERS --- *)
    
    /// <inheritdoc />
    let validateWithErrorMessage (db: LiteDatabase) =
        true, [""] |> Seq.ofList
        
    /// <inheritdoc />
    let validate (db: LiteDatabase) =
        true
    
    /// <inheritdoc />
    let deserialize (db: LiteDatabase)  =
        {
            GlobalAttributes = Map.empty
            Events = Map.empty
            Objects = Map.empty
        }
    
    /// <inheritdoc />
    let serialize (db: LiteDatabase) (log: OcelLog) =
        (*BsonMapper.Global.Entity<string * OcelEvent>()
            .Id(fun (id, _) -> id)
            .Field((fun (id, event) -> "", ""), "")
            |> ignore*)

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

        // Custom OcelValue handler to reduce size of stored data
        BsonMapper.Global.RegisterType<OcelValue>(
            fun value ->
                let doc = BsonDocument()
                doc["type"] <- 
                    match value with
                    | OcelString _ -> "s"
                    | OcelTimestamp _ -> "t"
                    | OcelInteger _ -> "i"
                    | OcelFloat _ -> "f"
                    | OcelBoolean _ -> "b"
                doc["val"] <- value.ToString()
                doc
            ,
            fun doc -> OcelString "" // TODO
        )

        // TODO
        BsonMapper.Global.RegisterType<OcelAttributes>(
            fun map ->
                let doc = BsonDocument()
                doc["test"] <- "test"
                doc
            ,
            fun doc -> Map.empty
        )

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
            fun doc -> doc["_id"].AsString, { Activity = ""; Timestamp = DateTimeOffset.Now; OMap = []; VMap = Map.empty })

        let eventsColl = db.GetCollection<string * OcelEvent>("events")
        log.Events |> Map.toSeq |> eventsColl.InsertBulk |> ignore

        let objectsColl = db.GetCollection<string * OcelObject>("objects")
        log.Objects |> Map.toSeq |> objectsColl.InsertBulk |> ignore

        let othersColl = db.GetCollection<string * OcelValue>("others")
        log.GlobalAttributes |> Map.toSeq |> othersColl.InsertBulk |> ignore