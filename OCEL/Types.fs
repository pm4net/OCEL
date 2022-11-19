namespace OCEL.Types

open System

type Value =
    | OcelString of string
    | OcelTimestamp of DateTimeOffset
    | OcelInteger of int64
    | OcelFloat of double
    | OcelBoolean of bool

type Attributes = Map<string, Value>

type Event = {
    Id: string
    Activity: string
    Timestamp: DateTimeOffset
    OMap: string seq
    VMap: Attributes
}

type Object = {
    Id: string
    Type: string
    OvMap: Attributes
}

type GlobalLog = {
    Attributes: Attributes
    AttributeNames: string seq
    ObjectTypes: string seq
}

type Log = {
    GlobalLog: GlobalLog
    Events: Event seq
    Objects: Object seq
}
