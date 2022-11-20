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

type OcelLogInfo = {
    Attributes: OcelAttributes
    AttributeNames: string seq
    ObjectTypes: string seq
}

type OcelLog = {
    LogInfo: OcelLogInfo
    Events: Map<string, OcelEvent>
    Objects: Map<string, OcelObject>
}

// Validations

type OcelLog with
    member this.IsValid() =
        let doObjectTypesMatchLogInfo =
            let distinctObjectTypes = 
                this.Objects
                |> Seq.map (fun o -> o.Value.Type)
                |> Set.ofSeq

            this.LogInfo.ObjectTypes
            |> Set.ofSeq = distinctObjectTypes

        let areAllAttributesInEventsAndObjectsInLogInfo =
            true // TODO

        doObjectTypesMatchLogInfo &&
        areAllAttributesInEventsAndObjectsInLogInfo
