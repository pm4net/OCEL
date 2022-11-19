namespace OCEL

open OCEL.Types

open System
open Newtonsoft.Json
open Newtonsoft.Json.Schema
open Newtonsoft.Json.Linq

module Json =
    [<Literal>]
    let private SchemaJson = """{"$schema":"http://json-schema.org/schema#","additionalProperties":true,"definitions":{"AttributeBooleanType":{"type":"boolean"},"AttributeDateType":{"type":"string","format":"date-time"},"AttributeFloatType":{"type":"number"},"AttributeIntType":{"type":"integer"},"AttributeStringType":{"type":"string"},"ObjectMappingType":{"type":"object"},"ValueMappingType":{"type":"object"},"EventType":{"properties":{"ocel:id":{"$ref":"#/definitions/AttributeStringType"},"ocel:activity":{"$ref":"#/definitions/AttributeStringType"},"ocel:timestamp":{"$ref":"#/definitions/AttributeDateType"},"ocel:vmap":{"items":{"$ref":"#/definitions/ValueMappingType"},"type":"object"},"ocel:omap":{"type":"array"}},"required":["ocel:id","ocel:activity","ocel:timestamp","ocel:omap","ocel:vmap"],"type":"object"},"ObjectType":{"properties":{"ocel:id":{"$ref":"#/definitions/AttributeStringType"},"ocel:type":{"$ref":"#/definitions/AttributeStringType"},"ocel:ovmap":{"items":{"$ref":"#/definitions/ValueMappingType"},"type":"object"}},"required":["ocel:id","ocel:type","ocel:ovmap"],"type":"object"}},"description":"Schema for the JSON-OCEL implementation","properties":{"ocel:events":{"items":{"$ref":"#/definitions/EventType"},"type":"object"},"ocel:objects":{"items":{"$ref":"#/definitions/ObjectMappingType"},"type":"object"}},"type":"object"}"""
    let private Schema = JSchema.Parse(SchemaJson)

    let private extractGlobalLog (jObj: JObject) : GlobalLog option =
        let extractAttributeNames (props: IJEnumerable<JProperty>) : string list =
            []

        let extractObjectTypes (props: IJEnumerable<JProperty>) : string list =
            []

        let extractAttributes (props: IJEnumerable<JProperty>) : Attributes =
            Map.empty

        let jGlobalLog = jObj["ocel:global-log"] |> Option.ofObj
        match jGlobalLog with
        | None -> None
        | Some token ->
            let props = token.Children<JProperty>()
            Some {
                Attributes = extractAttributes props
                AttributeNames = extractAttributeNames props
                ObjectTypes = extractObjectTypes props
            }

    let private extractGlobalEvent (jObj: JObject) : Event option =
        None

    let private extractGlobalObject (jObj: JObject) : OCEL.Types.Object option =
        None

    let Deserialize json : OcelLog option =
        let jObj = JObject.Parse json
        match jObj.IsValid Schema with
        | false -> None
        | true ->
            Some {
                GlobalLog =
                    match extractGlobalLog jObj with
                    | Some globalLog -> globalLog
                    | None -> failwith "No global log defined."
                GlobalEvent =
                    match extractGlobalEvent jObj with
                    | Some globalEvent -> globalEvent
                    | None -> failwith "No global event defined."
                GlobalObject =
                    match extractGlobalObject jObj with
                    | Some globalObject -> globalObject
                    | None -> failwith "No global object defined."
                Events = []
                Objects = []
            }

    let Validate json =
        JObject.Parse(json).IsValid

    let ValidateWithErrorMessages json =
        let jObj = JObject.Parse json
        let mutable errors : System.Collections.Generic.IList<string> = Array.empty
        let valid = jObj.IsValid(Schema, &errors)
        (valid, errors :> seq<_>)