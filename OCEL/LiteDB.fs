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
        db