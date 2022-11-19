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
    Id: string
    Activity: string
    Timestamp: DateTimeOffset
    OMap: string seq
    VMap: OcelAttributes
}

type OcelObject = {
    Id: string
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
    Events: OcelEvent seq
    Objects: OcelObject seq
}
