namespace OCEL

open OCEL.Types

open System
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Schema
open Newtonsoft.Json.Linq

module OcelJson =

    (* --- PRIVATE MEMBERS --- *)

    [<Literal>]
    let private SchemaJson = """{"$schema":"http://json-schema.org/schema#","additionalProperties":true,"definitions":{"AttributeBooleanType":{"type":"boolean"},"AttributeDateType":{"type":"string","format":"date-time"},"AttributeFloatType":{"type":"number"},"AttributeIntType":{"type":"integer"},"AttributeStringType":{"type":"string"},"ObjectMappingType":{"type":"object"},"ValueMappingType":{"type":"object"},"EventType":{"properties":{"ocel:id":{"$ref":"#/definitions/AttributeStringType"},"ocel:activity":{"$ref":"#/definitions/AttributeStringType"},"ocel:timestamp":{"$ref":"#/definitions/AttributeDateType"},"ocel:vmap":{"items":{"$ref":"#/definitions/ValueMappingType"},"type":"object"},"ocel:omap":{"type":"array"}},"required":["ocel:id","ocel:activity","ocel:timestamp","ocel:omap","ocel:vmap"],"type":"object"},"ObjectType":{"properties":{"ocel:id":{"$ref":"#/definitions/AttributeStringType"},"ocel:type":{"$ref":"#/definitions/AttributeStringType"},"ocel:ovmap":{"items":{"$ref":"#/definitions/ValueMappingType"},"type":"object"}},"required":["ocel:id","ocel:type","ocel:ovmap"],"type":"object"}},"description":"Schema for the JSON-OCEL implementation","properties":{"ocel:events":{"items":{"$ref":"#/definitions/EventType"},"type":"object"},"ocel:objects":{"items":{"$ref":"#/definitions/ObjectMappingType"},"type":"object"}},"type":"object"}"""
    let private Schema = JSchema.Parse(SchemaJson)

    /// Extract a token from a list of properties, and encapsulate the result in an Option in case it doesn't exist.
    let private extractOptionalTokenFromProperties (props: IJEnumerable<JProperty>) propName =
        match props |> Seq.tryFind (fun p -> p.Name = propName) with
        | Some p ->
            match p.First |> Option.ofObj with
            | Some t -> Some t
            | None -> None
        | None -> None

    /// Extract a token from a list of properties, and fail if the token can't be found.
    let private extractTokenFromProperties props propName eventId =
        match extractOptionalTokenFromProperties props propName with
        | Some token -> token
        | None -> failwith $"Object \"{eventId}\" is either not defined or has no value for \"{propName}\""

    /// Extract the value from a token and cast it into one of the supported OCEL types, otherwise throw an error.
    let private extractValueFromToken (token: JToken) =
        match token.Type with
        | JTokenType.Integer -> OcelInteger(token.Value<int64>())
        | JTokenType.Float -> OcelFloat(token.Value<double>())
        | JTokenType.String -> OcelString(token.Value<string>())
        | JTokenType.Boolean -> OcelBoolean(token.Value<bool>())
        | JTokenType.Date -> OcelTimestamp(token.Value<DateTimeOffset>())
        | _ -> failwith $"Type {token.Type} on attributes not supported."

    /// Extract a map of values from a list of properties
    let private extractValueMap (props: IJEnumerable<JProperty>) name =
        match extractOptionalTokenFromProperties props name with
        | None -> Map.empty
        | Some t ->
            t.Children<JProperty>()
            |> Seq.map (fun p -> p.Name, extractValueFromToken p.First)
            |> Map.ofSeq

    /// Extract a string array from a property with a given name
    let private extractStringArray (props: IJEnumerable<JProperty>) name =
        match props |> Seq.tryFind (fun p -> p.Name = name) with
        | None -> failwith $"No \"{name}\" defined."
        | Some p when p.First = null || p.First.Type <> JTokenType.Array -> failwith $"Propery \"{name}\" is not an array."
        | Some p -> p.First |> Seq.map (fun x -> x.Value<string>())

    /// Extract attributes that are not the attribute names and object types from the global log object
    let private extractAttributesFromGlobalLog (jObj: JObject) =
        match jObj["ocel:global-log"] |> Option.ofObj with
        | None -> Map.empty
        | Some token ->
            token.Children<JProperty>()
            |> Seq.filter (fun p -> p.Name <> "ocel:attribute-names" && p.Name <> "ocel:object-types")
            |> Seq.map (fun p ->
                match p.First |> Option.ofObj with
                | None -> failwith $"Property {p.Name} has no value."
                | Some t -> (p.Name, extractValueFromToken t))
            |> Map.ofSeq

    /// Extract information from an object, given some extractor function
    let private extractFromObject (jObj: JObject) name extractor =
        match jObj[name] |> Option.ofObj with
        | None -> failwith $"No \"{name}\" defined."
        | Some token ->
            token.Children<JProperty>()
            |> Seq.map (fun rootProp ->
                match rootProp.First |> Option.ofObj with
                | None -> failwith $"Propery {rootProp.Name} does not have a value."
                | Some t ->
                    let props = t.Children<JProperty>()
                    (rootProp.Name, extractor rootProp props)
            )
            |> Map.ofSeq

    /// Extract all events from an OCEL log
    let private extractEvents jObj =
        let extractor (p: JProperty) props = {
            Activity = (extractTokenFromProperties props "ocel:activity" p.Name).Value<string>()
            Timestamp = (extractTokenFromProperties props "ocel:timestamp" p.Name).Value<DateTimeOffset>()
            OMap = extractStringArray props "ocel:omap"
            VMap = extractValueMap props "ocel:vmap"
        }
        extractFromObject jObj "ocel:events" extractor

    /// Extract all objects from an OCEL log
    let private extractObjects jObj =
        let extractor (p: JProperty) props = {
            Type = (extractTokenFromProperties props "ocel:type" p.Name).Value<string>()
            OvMap = extractValueMap props "ocel:ovmap"
        }
        extractFromObject jObj "ocel:objects" extractor

    /// Parse an OCEL JSON string by handling dates as DateTimeOffset
    let private parseWithDateTimeOffsetHandling json =
        let reader = new JsonTextReader(new StringReader(json))
        reader.DateParseHandling <- DateParseHandling.DateTimeOffset
        JObject.Load reader

    /// Validate a JSON string against the OCEL JSON schema, with error messages
    let private validateJObjectWithErrorMessages (jObj: JObject) =
        let mutable errors : System.Collections.Generic.IList<string> = Array.empty
        let valid = jObj.IsValid(Schema, &errors)
        valid, errors :> seq<_>

    (* --- PUBLIC MEMBERS --- *)

    /// <inheritdoc />
    let validateWithErrorMessages json =
        let jObj = parseWithDateTimeOffsetHandling json
        validateJObjectWithErrorMessages jObj

    /// <inheritdoc />
    let validate json =
        (parseWithDateTimeOffsetHandling json).IsValid Schema

    /// <inheritdoc />
    let deserialize json =
        let jObj = parseWithDateTimeOffsetHandling json
        match validateJObjectWithErrorMessages jObj with
        | false, errors -> failwith $"""JSON not validated by schema. Errors:{Environment.NewLine}{errors |> String.concat Environment.NewLine}."""
        | true, _ ->
            {
                GlobalAttributes = extractAttributesFromGlobalLog jObj
                Events = extractEvents jObj
                Objects = extractObjects jObj
            }
    
    /// <inheritdoc />
    let serialize (formatting: OCEL.Types.Formatting) (log: OcelLog) : string =
        /// Get the value from the DU and put it into a JToken. Using reflection as FromObject handles the correct typing
        let createTokenFromOcelValue value =
            match value with
            | OcelString s -> JToken.FromObject s
            | OcelTimestamp t -> JToken.FromObject t
            | OcelInteger i -> JToken.FromObject i
            | OcelFloat f -> JToken.FromObject f
            | OcelBoolean b -> JToken.FromObject b

        /// Construct the global log object
        let createGlobalLog log =
            let jGlob = JObject()
            log.GlobalAttributes |> Map.iter (fun k v -> jGlob[k] <- createTokenFromOcelValue v)
            jGlob["ocel:attribute-names"] <- JArray(log.AttributeNames)
            jGlob["ocel:object-types"] <- JArray(log.ObjectTypes)
            jGlob

        /// Construct an object with tokens mapping an ID to a value
        let createIdValueMap map =
            let jV = JObject()
            map |> Map.iter (fun k v -> jV[k] <- createTokenFromOcelValue v)
            jV

        /// Create a single event from an OCEL event
        let createEvent event =
            let jEv = JObject()
            jEv["ocel:activity"] <- event.Activity
            jEv["ocel:timestamp"] <- event.Timestamp
            jEv["ocel:omap"] <- JArray(event.OMap)
            jEv["ocel:vmap"] <- JObject(createIdValueMap event.VMap)
            jEv

        /// Create the list of events
        let createEvents log =
            let jEv = JObject()
            log.Events |> Seq.iter (fun e -> jEv[e.Key] <- createEvent e.Value)
            jEv

        /// Create a single object from an OCEL object
        let createObject obj =
            let jObj = JObject()
            jObj["ocel:type"] <- obj.Type
            jObj["ocel:ovmap"] <- createIdValueMap obj.OvMap
            jObj

        /// Create the list of objects
        let createObjects log =
            let jObj = JObject()
            log.Objects |> Seq.iter (fun e -> jObj[e.Key] <- createObject e.Value)
            jObj

        /// Convert custom formatting enum to Newtonsoft formatting enum
        let toNewtonSoftFormatting formatting =
            match formatting with
            | Types.Formatting.None -> Newtonsoft.Json.Formatting.None
            | Types.Formatting.Indented -> Newtonsoft.Json.Formatting.Indented
            | _ -> raise (ArgumentOutOfRangeException(nameof(formatting)))

        if not log.IsValid then
            failwith "Log is invalid."

        let jObj = new JObject()
        jObj["ocel:global-log"] <- createGlobalLog log
        jObj["ocel:global-event"] <- JObject([
            JProperty("ocel:id", "__INVALID__")
            JProperty("ocel:activity", "__INVALID__")
            JProperty("ocel:timestamp", "__INVALID__")
            JProperty("ocel:omap", JArray())
            JProperty("ocel:vmap", JObject())
        ])
        jObj["ocel:global-object"] <- JObject([
            JProperty("ocel:id", "__INVALID__")
            JProperty("ocel:type", "__INVALID__")
            JProperty("ocel:ovmap", JObject())
        ])
        jObj["ocel:events"] <- createEvents log
        jObj["ocel:objects"] <- createObjects log
        
        jObj.ToString (formatting |> toNewtonSoftFormatting)