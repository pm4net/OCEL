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
    | OcelNull

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

    /// Compare a log to this log. Useful when handling this type from C# and needing to check for structural-, not reference-equality.
    member this.IsEqual other =
        this = other

    /// Merge a log with another log. Duplicate keys are overwritten by the other log.
    member this.MergeWith other =
        let mergeMaps a b =
            (a, b) ||> Map.fold (fun acc key value -> Map.add key value acc)

        { this with
            GlobalAttributes = (this.GlobalAttributes, other.GlobalAttributes) ||> mergeMaps
            Events = (this.Events, other.Events) ||> mergeMaps
            Objects = (this.Objects, other.Objects) ||> mergeMaps
        }

    /// Merge duplicate objects and update the object ID's on the events that reference them.
    /// This is useful when the same object is repeatedly added without the ability to detect it efficiently, such as in logging.
    member this.MergeDuplicateObjects () =

        // Group by the object itself, and extract all ID's that reference this same object
        let uniqueObjsAndReferencingIds = this.Objects |> Map.toSeq |> Seq.groupBy snd |> Seq.map (fun (uniqueObj, objs) -> uniqueObj, objs |> Seq.map fst)
        let uniqueObjsAndReferencingIdsMap = uniqueObjsAndReferencingIds |> Map.ofSeq

        // Create updated mapping where each object only appears once, and has the ID of the first encountered version of it
        let updatedObjs = uniqueObjsAndReferencingIds |> Seq.map (fun (o, ids) -> ids |> Seq.head, o) |> Map.ofSeq

        // Create updated event mapping where each object reference is updated to reflect the new ID of an object, in case it was removed due to duplication
        let updatedEvents =
            this.Events
            |> Map.toSeq
            |> Seq.map (fun (id, e) ->
                id,
                { e with 
                    OMap =
                        e.OMap
                        |> List.choose (fun o ->
                            // For each object reference, find the list of objects that contains the ID. 
                            // Then simply take the first ID and use it, since we previously always picked the first ID for duplicate objects.
                            match this.Objects.TryFind o with
                            | Some oldObj ->
                                match uniqueObjsAndReferencingIdsMap.TryFind oldObj with
                                | Some updatedObjIds -> updatedObjIds |> Seq.head |> Some
                                | None -> None
                            | None -> None)
                }
            )
            |> Map.ofSeq

        { this with
            Events = updatedEvents
            Objects = updatedObjs
        }

    /// Convert a list of object types to attributes by moving objects to all events that reference them.
    member this.ConvertObjectsToAttributes objectTypes =

        /// Add an object as attributes to all events that reference the object ID
        let addObjectsToEvents (objects: Map<string, OcelObject>) (log: OcelLog) =

            // Already convert objects now as they are likely referenced by several events
            let convertedObjs = objects |> Map.map (fun _ o ->
                // If object only has one attribute, we don't need to use a map for it
                match o.OvMap |> Map.count with
                | 1 -> o.OvMap.Values |> Seq.head
                | _ -> OcelMap(o.OvMap)
            )

            { log with
                Events = 
                    log.Events
                    |> Map.map (fun _ e ->
                        let toConvert = e.OMap |> List.filter (fun o -> objects.ContainsKey o)
                        match toConvert with
                        | [] -> e
                        | _ ->
                            let updatedVMap = (e.VMap, toConvert) ||> List.fold (fun attrs objId -> attrs |> Map.add (objects[objId].Type) convertedObjs[objId])
                            let updatedOMap = e.OMap |> List.except toConvert // Remove objects that were converted to attributes
                            { e with
                                OMap = updatedOMap
                                VMap = updatedVMap
                            }
                        )
            }

        /// Remove a map of objects from the log
        let removeObjects (objects: Map<string, OcelObject>) (log: OcelLog) =
            { log with Objects = (log.Objects, objects) ||> Map.fold (fun map objId _ -> map |> Map.remove objId) }
        
        let objTypes = objectTypes |> Set.ofList
        let objsToConvert = this.Objects |> Map.filter (fun _ o -> objTypes |> Set.contains o.Type)
        (objsToConvert, this) ||> addObjectsToEvents |> removeObjects objsToConvert

    /// Convert a list of attributes to objects by moving attributes to new objects and add references to the events.
    member this.ConvertAttributesToObjects attributes =

        /// Move an attribute from an event to the object map, removing the attribute from itself and adding a reference to the object instead
        let moveAttrFromEventToObject (id, event: OcelEvent) attr (log: OcelLog) =
            let objId = Guid.NewGuid().ToString()
            let newObj = {
                Type = attr
                OvMap = 
                    match event.VMap[attr] with
                    | OcelMap m -> m
                    | _ -> [attr, event.VMap[attr]] |> Map.ofList
            }

            { log with
                Events =  log.Events |> Map.change id (fun _ -> Some { event with VMap = event.VMap |> Map.remove attr; OMap = objId :: event.OMap })
                Objects = log.Objects |> Map.add objId newObj
            }

        /// Find all events that have the attribute, and add them to the log as objects
        let convertAttributesToObjects attr (log: OcelLog) =
            let events = log.Events |> Map.filter (fun _ e -> e.VMap.ContainsKey attr) |> Map.toList
            (log, events) ||> List.fold (fun log event -> moveAttrFromEventToObject event attr log)

        (this, attributes) ||> List.fold (fun log attr -> convertAttributesToObjects attr log) |> fun log -> log.MergeDuplicateObjects()

    /// An empty log
    static member Empty =
        {
            GlobalAttributes = Map.empty
            Events = Map.empty
            Objects = Map.empty
        }

type OcelEvent with
    /// Compare a log to this log. Useful when handling this type from C# and needing to check for structural-, not reference-equality.
    member this.IsEqual other =
        this = other

type OcelObject with
    /// Compare a log to this log. Useful when handling this type from C# and needing to check for structural-, not reference-equality.
    member this.IsEqual other =
        this = other

type OcelValue with
    /// Compare a log to this log. Useful when handling this type from C# and needing to check for structural-, not reference-equality.
    member this.IsEqual other =
        this = other