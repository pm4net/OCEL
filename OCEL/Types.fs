namespace OCEL.Types

open System

// Auxilliary types

type Formatting =
    | None = 0
    | Indented = 1

// OCEL types

type OcelValue =
    | OcelString of string
    | OcelTimestamp of DateTimeOffset
    | OcelInteger of int64
    | OcelFloat of double
    | OcelBoolean of bool
    | OcelList of OcelValue list
    | OcelMap of OcelAttributes 

and OcelAttributes = Map<string, OcelValue>

type OcelEvent = {
    Activity: string
    Timestamp: DateTimeOffset
    OMap: string list
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
        let rec extractKeysFromValue v =
            match v with
            | OcelList l -> l |> List.collect extractKeysFromValue
            | OcelMap m -> m |> extractKeysFromMapping
            | _ -> []

        and extractKeysFromMapping attrs =
            attrs
            |> Map.toList
            |> List.map (fun (k, v) -> k :: extractKeysFromValue v)
            |> List.concat

        let eventAttributes =
            this.Events
            |> Map.toList
            |> List.map (fun (_, v) -> v.VMap |> extractKeysFromMapping)
            |> List.concat

        let objectAttributes =
            this.Objects
            |> Map.toList
            |> List.map (fun (_, v) -> v.OvMap |> extractKeysFromMapping)
            |> List.concat
        
        Set.ofList eventAttributes + Set.ofList objectAttributes

    /// The set of all object types in the log
    member this.ObjectTypes =
        this.Objects
        |> Map.toList
        |> List.map (fun (_, v) -> v.Type)
        |> Set.ofList

    /// The list of events and their ID's, ordered by their timestamp
    member this.OrderedEvents =
        this.Events
        |> Map.toList
        |> List.sortBy (fun (_, v) -> v.Timestamp)

    /// Indicates whether the log is valid according to the OCEL specification
    member this.IsValid =
        /// Objects that are referenced by events must exist
        let doAllObjectsReferencedInEventsExist =
            this.Events
            |> Map.toList
            |> List.forall (fun (k, v) -> v.OMap |> List.forall (fun o -> this.Objects.TryFind o <> None))

        doAllObjectsReferencedInEventsExist

    /// Merge a log with another log. Duplicate keys are overwritten by the other log.
    member this.MergeWith other =
        let mergeMaps a b =
            (a, b) ||> Map.fold (fun acc key value -> Map.add key value acc)

        { this with
            GlobalAttributes = (this.GlobalAttributes, other.GlobalAttributes) ||> mergeMaps
            Events = (this.Events, other.Events) ||> mergeMaps
            Objects = (this.Objects, other.Objects) ||> mergeMaps
        }

    /// An empty log
    static member Empty =
        {
            GlobalAttributes = Map.empty
            Events = Map.empty
            Objects = Map.empty
        }
