namespace OCEL

open LiteDB
open OCEL.Types

module LiteDB =
    
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
        let eventsColl = db.GetCollection<string * OcelEvent>("events")
        log.Events |> Map.toSeq |> eventsColl.InsertBulk |> ignore
        let objectsColl = db.GetCollection<string * OcelObject>("objects")
        log.Objects |> Map.toSeq |> objectsColl.InsertBulk |> ignore
        let othersColl = db.GetCollection<string * OcelValue>("others")
        log.GlobalAttributes |> Map.toSeq |> othersColl.InsertBulk |> ignore