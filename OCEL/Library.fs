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

    let private extractAttributes (props: IJEnumerable<JProperty>) : Attributes =
        props
        |> Seq.filter (fun p -> p.Name <> "ocel:attribute-names" && p.Name <> "ocel:object-types")
        |> Seq.map (fun p ->
            match p.First |> Option.ofObj with
            | None -> failwith $"Property {p.Name} has no value."
            | Some t ->
                match t.Type with
                | JTokenType.Integer -> (p.Name, OcelInteger(t.Value<int64>()))
                | JTokenType.Float -> (p.Name, OcelFloat(t.Value<double>()))
                | JTokenType.String -> (p.Name, OcelString(t.Value<string>()))
                | JTokenType.Boolean -> (p.Name, OcelBoolean(t.Value<bool>()))
                | JTokenType.Date -> (p.Name, OcelTimestamp(t.Value<DateTimeOffset>()))
                | _ -> raise (ArgumentOutOfRangeException(nameof(p), $"Type {p.Value.Type} on attributes not supported."))
        )
        |> Map.ofSeq

    let private extractStringArray (props: IJEnumerable<JProperty>) name =
        match props |> Seq.tryFind (fun p -> p.Name = name) with
        | None -> failwith $"No \"{name}\" inside global log defined."
        | Some p when p.First = null || p.First.Type <> JTokenType.Array -> failwith $"Propery \"{name}\" is not an array."
        | Some p -> p.First |> Seq.map (fun x -> x.Value<string>())

    let private extractGlobalLog (jObj: JObject) : GlobalLog option =
        let jGlobalLog = jObj["ocel:global-log"] |> Option.ofObj
        match jGlobalLog with
        | None -> None
        | Some token ->
            let props = token.Children<JProperty>()
            Some {
                Attributes = extractAttributes props
                AttributeNames = extractStringArray props "ocel:attribute-names"
                ObjectTypes = extractStringArray props "ocel:object-types"
            }

    let Validate json =
        JObject.Parse(json).IsValid Schema

    let private ValidateJObjectWithErrorMessages (jObj: JObject) =
        let mutable errors : System.Collections.Generic.IList<string> = Array.empty
        let valid = jObj.IsValid(Schema, &errors)
        (valid, errors :> seq<_>)

    let ValidateWithErrorMessages json =
        let jObj = JObject.Parse json
        ValidateJObjectWithErrorMessages jObj

    let Deserialize json : Log =
        let jObj = JObject.Parse json
        match ValidateJObjectWithErrorMessages jObj with
        | false, errors -> failwith $"JSON not validated by schema. Errors: {errors |> Seq.map (fun e -> e + Environment.NewLine)}."
        | true, _ ->
            {
                GlobalLog =
                    match extractGlobalLog jObj with
                    | Some globalLog -> globalLog
                    | None -> failwith """No "ocel:global-log" defined."""
                Events = []
                Objects = []
            }
    
    let Serialize (log: Log) : string =
        ""