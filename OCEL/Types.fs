namespace OCEL.Types

open System

type Value =
    | String of string
    | Timestamp of DateTimeOffset
    | Integer of int64
    | Float of double
    | Boolean of bool

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

type OcelLog = {
    GlobalLog: GlobalLog
    GlobalEvent: Event
    GlobalObject: Object
    Events: Event seq
    Objects: Object seq
}
