namespace OCEL

open OCEL.Types

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Schema
open Newtonsoft.Json.Linq

module Json =
    [<Literal>]
    let private SchemaJson = """{"$schema":"http://json-schema.org/schema#","additionalProperties":true,"definitions":{"AttributeBooleanType":{"type":"boolean"},"AttributeDateType":{"type":"string","format":"date-time"},"AttributeFloatType":{"type":"number"},"AttributeIntType":{"type":"integer"},"AttributeStringType":{"type":"string"},"ObjectMappingType":{"type":"object"},"ValueMappingType":{"type":"object"},"EventType":{"properties":{"ocel:id":{"$ref":"#/definitions/AttributeStringType"},"ocel:activity":{"$ref":"#/definitions/AttributeStringType"},"ocel:timestamp":{"$ref":"#/definitions/AttributeDateType"},"ocel:vmap":{"items":{"$ref":"#/definitions/ValueMappingType"},"type":"object"},"ocel:omap":{"type":"array"}},"required":["ocel:id","ocel:activity","ocel:timestamp","ocel:omap","ocel:vmap"],"type":"object"},"ObjectType":{"properties":{"ocel:id":{"$ref":"#/definitions/AttributeStringType"},"ocel:type":{"$ref":"#/definitions/AttributeStringType"},"ocel:ovmap":{"items":{"$ref":"#/definitions/ValueMappingType"},"type":"object"}},"required":["ocel:id","ocel:type","ocel:ovmap"],"type":"object"}},"description":"Schema for the JSON-OCEL implementation","properties":{"ocel:events":{"items":{"$ref":"#/definitions/EventType"},"type":"object"},"ocel:objects":{"items":{"$ref":"#/definitions/ObjectMappingType"},"type":"object"}},"type":"object"}"""
    let private Schema = JSchema.Parse(SchemaJson)

    let private extractStringArray (props: IJEnumerable<JProperty>) name =
        match props |> Seq.tryFind (fun p -> p.Name = name) with
        | None -> failwith $"No \"{name}\" defined."
        | Some p when p.First = null || p.First.Type <> JTokenType.Array -> failwith $"Propery \"{name}\" is not an array."
        | Some p -> p.First |> Seq.map (fun x -> x.Value<string>())

    let private extractOptionalTokenFromProperties (props: IJEnumerable<JProperty>) propName =
        match props |> Seq.tryFind (fun p -> p.Name = propName) with
        | Some p ->
            match p.First |> Option.ofObj with
            | Some t -> Some t
            | None -> None
        | None -> None

    let private extractTokenFromProperties (props: IJEnumerable<JProperty>) propName eventId =
        match extractOptionalTokenFromProperties props propName with
        | Some token -> token
        | None -> failwith $"Token \"{propName}\" is either not defined or has no value for event \"{eventId}\""

    let private extractValueFromToken (token: JToken) =
        match token.Type with
        | JTokenType.Integer -> OcelInteger(token.Value<int64>())
        | JTokenType.Float -> OcelFloat(token.Value<double>())
        | JTokenType.String -> OcelString(token.Value<string>())
        | JTokenType.Boolean -> OcelBoolean(token.Value<bool>())
        | JTokenType.Date -> OcelTimestamp(token.Value<DateTimeOffset>())
        | _ -> failwith $"Type {token.Type} on attributes not supported."

    let private extractAttributesFromGlobalLog (props: IJEnumerable<JProperty>) : OcelAttributes =
        props
        |> Seq.filter (fun p -> p.Name <> "ocel:attribute-names" && p.Name <> "ocel:object-types")
        |> Seq.map (fun p ->
            match p.First |> Option.ofObj with
            | None -> failwith $"Property {p.Name} has no value."
            | Some t -> (p.Name, extractValueFromToken t))
        |> Map.ofSeq

    let private extractGlobalLog (jObj: JObject) : OcelLogInfo option =
        match jObj["ocel:global-log"] |> Option.ofObj with
        | None -> None
        | Some token ->
            let props = token.Children<JProperty>()
            Some {
                Attributes = extractAttributesFromGlobalLog props
                AttributeNames = extractStringArray props "ocel:attribute-names"
                ObjectTypes = extractStringArray props "ocel:object-types"
            }

    let private extractEvents (jObj: JObject) : Map<string, OcelEvent> =
        match jObj["ocel:events"] |> Option.ofObj with
        | None -> failwith """No "ocel:events" defined."""
        | Some token ->
            token.Children<JProperty>()
            |> Seq.map (fun p ->
                match p.First |> Option.ofObj with
                | None -> failwith $"Property {p.Name} does not have a value."
                | Some t ->
                    let props = t.Children<JProperty>()
                    (p.Name, {
                        Activity = (extractTokenFromProperties props "ocel:activity" p.Name).Value<string>()
                        Timestamp = (extractTokenFromProperties props "ocel:timestamp" p.Name).Value<DateTimeOffset>()
                        OMap = extractStringArray props "ocel:omap"
                        VMap = 
                            match extractOptionalTokenFromProperties props "ocel:vmap" with
                            | None -> Map.empty
                            | Some t ->
                                t.Children<JProperty>()
                                |> Seq.map (fun p -> p.Name, extractValueFromToken p.First)
                                |> Map.ofSeq
                    })
            )
            |> Map.ofSeq

    let private extractObjects (jObj: JObject) : Map<string, OcelObject> =
        Map.empty

    let private ParseWithDateTimeOffsetHandling json =
        let reader = new JsonTextReader(new StringReader(json))
        reader.DateParseHandling <- DateParseHandling.DateTimeOffset
        JObject.Load reader

    let Validate json =
        (ParseWithDateTimeOffsetHandling json).IsValid Schema

    let private ValidateJObjectWithErrorMessages (jObj: JObject) =
        let mutable errors : System.Collections.Generic.IList<string> = Array.empty
        let valid = jObj.IsValid(Schema, &errors)
        (valid, errors :> seq<_>)

    let ValidateWithErrorMessages json =
        let jObj = ParseWithDateTimeOffsetHandling json
        ValidateJObjectWithErrorMessages jObj

    /// Deserialize a JSON string into an OCEL log, and validate it against the OCEL schema.
    let Deserialize json : OcelLog =
        let jObj = ParseWithDateTimeOffsetHandling json
        match ValidateJObjectWithErrorMessages jObj with
        | false, errors -> failwith $"JSON not validated by schema. Errors: {errors |> Seq.map (fun e -> e + Environment.NewLine)}."
        | true, _ ->
            {
                LogInfo =
                    match extractGlobalLog jObj with
                    | Some globalLog -> globalLog
                    | None -> failwith """No "ocel:global-log" defined."""
                Events = extractEvents jObj
                Objects = extractObjects jObj
            }
    
    let Serialize (log: OcelLog) : string =
        ""