namespace OCEL.Types

open System

// Auxilliary types

type Formatting =
    | None = 0
    | Indented = 1

// OCEL types

[<CustomEquality; NoComparison>]
type OcelValue =
    | OcelString of string
    | OcelTimestamp of DateTimeOffset
    | OcelInteger of int64
    | OcelFloat of double
    | OcelBoolean of bool
    | OcelList of OcelValue seq
    | OcelMap of OcelAttributes 
    with
    override this.Equals(other) =
        match other with
        | :? OcelValue as v ->
            match this, v with
            | OcelString s1, OcelString s2 -> s1 = s2
            | OcelTimestamp t1, OcelTimestamp t2 -> t1 = t2
            | OcelInteger i1, OcelInteger i2 -> i1 = i2
            | OcelFloat f1, OcelFloat f2 -> f1 = f2
            | OcelBoolean b1, OcelBoolean b2 -> b1 = b2
            // Needs custom comparison of each item, because seq has reference equality
            | OcelList l1, OcelList l2 -> (l1, l2) ||> Seq.forall2 (fun a b -> a = b)
            | OcelMap m1, OcelMap m2 -> m1 = m2
            | _ -> false
        | _ -> false
    override this.GetHashCode() = hash this

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
            // Needs custom comparison of each item, because seq has reference equality
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
            |> Seq.map (fun (_, v) -> v.OvMap |> extractKeysFromMapping)
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
