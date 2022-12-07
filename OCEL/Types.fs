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
    | OcelList of OcelValue seq
    | OcelMap of OcelAttributes

and OcelAttributes = Map<string, OcelValue>

[<CustomEquality; NoComparison>]
type OcelEvent = {
    Activity: string
    Timestamp: DateTimeOffset
    OMap: string seq
    VMap: OcelAttributes
} with
    override this.Equals(other) =
        match other with
        | :? OcelEvent as e ->
            this.Activity = e.Activity &&
            this.Timestamp = e.Timestamp &&
            this.VMap = e.VMap &&
            (this.OMap, e.OMap) ||> Seq.compareWith Operators.compare = 0
        | _ -> false
    override this.GetHashCode() = hash this

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
            | OcelList l -> l |> Seq.collect extractKeysFromValue |> Seq.toList
            | OcelMap m -> m |> extractKeysFromMapping
            | _ -> []

        and extractKeysFromMapping attrs =
            attrs
            |> Map.toList
            |> List.map (fun (k, v) -> k :: extractKeysFromValue v)
            |> List.concat

        let eventAttributes =
            this.Events
            |> Map.toSeq
            |> Seq.map (fun (_, v) -> v.VMap |> extractKeysFromMapping)
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

    /// Indicates whether the log is valid according to the OCEL specification
    member this.IsValid =
        /// Objects that are referenced by events must exist
        let doAllObjectsReferencedInEventsExist =
            this.Events
            |> Seq.forall 
                (fun e -> 
                    e.Value.OMap
                    |> Seq.forall (fun o -> this.Objects.TryFind o <> None))

        doAllObjectsReferencedInEventsExist
