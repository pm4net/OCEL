namespace OCEL.Types

open System

type OcelValue =
    | OcelString of string
    | OcelTimestamp of DateTimeOffset
    | OcelInteger of int64
    | OcelFloat of double
    | OcelBoolean of bool

type OcelAttributes = Map<string, OcelValue>

type OcelEvent = {
    Activity: string
    Timestamp: DateTimeOffset
    OMap: string seq
    VMap: OcelAttributes
}

type OcelObject = {
    Type: string
    OvMap: OcelAttributes
}

type OcelLog = {
    GlobalAttributes: OcelAttributes
    Events: Map<string, OcelEvent>
    Objects: Map<string, OcelObject>
}

// Extending types with properties and functions

type OcelLog with
    member this.AttributeNames =
        Set.ofSeq [""]

    member this.ObjectTypes =
        Set.ofSeq [""]

    /// The list of events and their ID's, ordered by their timestamp
    member this.OrderedEvents =
        this.Events
        |> Map.toSeq
        |> Seq.sortBy (fun (_, v) -> v.Timestamp)

    member this.IsValid() =
        let doObjectTypesMatchLogInfo =
            let distinctObjectTypes = 
                this.Objects
                |> Seq.map (fun o -> o.Value.Type)
                |> Set.ofSeq

            this.ObjectTypes = distinctObjectTypes

        let areAllAttributesInEventsAndObjectsInLogInfo =
            true // TODO
            
        doObjectTypesMatchLogInfo &&
        areAllAttributesInEventsAndObjectsInLogInfo
