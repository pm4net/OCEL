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
    /// The set of all attribute names used in both events and objects in the log
    member this.AttributeNames =
        let eventAttributes =
            this.Events
            |> Map.toSeq
            |> Seq.map (fun (_, v) -> v.VMap.Keys)
            |> Seq.concat

        let objectAttributes =
            this.Objects
            |> Map.toSeq
            |> Seq.map (fun (_, v) -> v.OvMap.Keys)
            |> Seq.concat
        
        Set.ofSeq eventAttributes + Set.ofSeq objectAttributes

    /// The set of all object types in the log
    member this.ObjectTypes =
        this.Objects
        |> Map.toSeq
        |> Seq.map (fun (_, v) -> v.Type)
        |> Set.ofSeq

    /// The list of events and their ID's, ordered by their timestamp
    member this.OrderedEvents =
        this.Events
        |> Map.toSeq
        |> Seq.sortBy (fun (_, v) -> v.Timestamp)
