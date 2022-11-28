﻿namespace OCEL

open System
open System.Globalization
open System.IO
open System.Xml.Schema
open System.Xml.Linq
open FSharp.Data
open OCEL.Types

module Xml =

    (* --- PRIVATE MEMBERS --- *)

    [<Literal>]
    let private SchemaXml = """<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" elementFormDefault="qualified"><xs:element name="log" type="LogType"/><xs:complexType name="AttributableType"><xs:choice minOccurs="0" maxOccurs="unbounded"><xs:element name="string" minOccurs="0" maxOccurs="unbounded" type="AttributeStringType"/><xs:element name="date" minOccurs="0" maxOccurs="unbounded" type="AttributeDateType"/><xs:element name="int" minOccurs="0" maxOccurs="unbounded" type="AttributeIntType"/><xs:element name="float" minOccurs="0" maxOccurs="unbounded" type="AttributeFloatType"/><xs:element name="boolean" minOccurs="0" maxOccurs="unbounded" type="AttributeBooleanType"/><xs:element name="id" minOccurs="0" maxOccurs="unbounded" type="AttributeIDType"/><xs:element name="list" minOccurs="0" maxOccurs="unbounded" type="AttributeListType"/><xs:element name="container" minOccurs="0" maxOccurs="unbounded" type="AttributeContainerType"/></xs:choice></xs:complexType><xs:complexType name="AttributeStringType"><xs:complexContent><xs:extension base="AttributeType"><xs:attribute name="value" use="required" type="xs:string"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="AttributeDateType"><xs:complexContent><xs:extension base="AttributeType"><xs:attribute name="value" use="required" type="xs:dateTime"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="AttributeIntType"><xs:complexContent><xs:extension base="AttributeType"><xs:attribute name="value" use="required" type="xs:long"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="AttributeFloatType"><xs:complexContent><xs:extension base="AttributeType"><xs:attribute name="value" use="required" type="xs:double"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="AttributeBooleanType"><xs:complexContent><xs:extension base="AttributeType"><xs:attribute name="value" use="required" type="xs:boolean"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="AttributeIDType"><xs:complexContent><xs:extension base="AttributeType"><xs:attribute name="value" use="required" type="xs:string"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="AttributeListType"><xs:complexContent><xs:extension base="AttributeType"/></xs:complexContent></xs:complexType><xs:complexType name="AttributeContainerType"><xs:complexContent><xs:extension base="AttributableType"/></xs:complexContent></xs:complexType><xs:complexType name="GlobalsType"><xs:complexContent><xs:extension base="AttributableType"><xs:attribute name="scope" type="xs:NCName" use="required"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="AttributeType"><xs:complexContent><xs:extension base="AttributableType"><xs:attribute name="key" use="required" type="xs:Name"/></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="ElementType"><xs:complexContent><xs:extension base="AttributableType"/></xs:complexContent></xs:complexType><xs:complexType name="LogType"><xs:complexContent><xs:extension base="ElementType"><xs:sequence><xs:element name="global" minOccurs="0" maxOccurs="4" type="GlobalsType"/><xs:element name="events" minOccurs="0" maxOccurs="unbounded" type="EventsType"/><xs:element name="objects" minOccurs="0" maxOccurs="unbounded" type="ObjectsType"/></xs:sequence></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="EventsType"><xs:complexContent><xs:extension base="ElementType"><xs:sequence><xs:element name="event" minOccurs="0" maxOccurs="unbounded" type="EventType"/></xs:sequence></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="EventType"><xs:complexContent><xs:extension base="ElementType"></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="ObjectsType"><xs:complexContent><xs:extension base="ElementType"><xs:sequence><xs:element name="object" minOccurs="0" maxOccurs="unbounded" type="ObjectType"/></xs:sequence></xs:extension></xs:complexContent></xs:complexType><xs:complexType name="ObjectType"><xs:complexContent><xs:extension base="ElementType"></xs:extension></xs:complexContent></xs:complexType></xs:schema>"""
    let private Schema = XmlSchema.Read(new StringReader(SchemaXml), null)

    /// Try to find the element where the key has a given value
    let private getElementWithKey keyValue (element: XElement) =
        element.Elements()
        |> Seq.tryFind (fun e -> e.Attribute("key").Value = keyValue)

    /// Get the value of an element that has a given key
    let private getStringValueOfElementWithKey keyValue (xElem: XElement) =
        match getElementWithKey keyValue xElem with
        | None -> failwith $"No element with key \"{keyValue}\" defined for element: {xElem}"
        | Some e ->
            match e.Attribute "value" |> Option.ofObj with
            | None -> failwith $"No attribute \"value\" defined for element: {e}"
            | Some v -> v.Value

    /// Extract the OCEL value from an attribute and try to find the correct type for it
    let private extractValueFromAttribute (xAttr: XAttribute) =
        match System.Int32.TryParse(xAttr.Value, NumberStyles.Integer, CultureInfo.InvariantCulture) with
        | true, int -> OcelInteger int
        | _ ->
            match System.Double.TryParse(xAttr.Value, NumberStyles.Float, CultureInfo.InvariantCulture) with
            | true, double -> OcelFloat double
            | _ ->
                match System.Boolean.TryParse xAttr.Value with
                | true, bool -> OcelBoolean bool
                | _ ->
                    match System.DateTimeOffset.TryParse(xAttr.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal) with
                    | true, dto -> OcelTimestamp dto
                    | _ -> OcelString xAttr.Value

    /// Extract a sequence of value from a list with a given name inside the given element, using some extractor function on 
    let extractArray (xElem: XElement) name extractor =
        match getElementWithKey name xElem with
        | None -> failwith $"No <{name}> defined for element: {xElem}"
        | Some l ->
            l.Elements()
            |> Seq.map extractor

    /// Get the key attribute of an element where the value of the key matches the input
    let private getKeyAttributeWithKeyValue name (xEl: XElement) =
        match xEl.Attribute "key" |> Option.ofObj with
        | Some attr ->
            match attr.Value = name with
            | true -> Some attr
            | _ -> None
        | None -> None

    /// Try to find the global element with a given scope
    let private globalWithScope scope (element: XElement) =
        element.Elements()
        |> Seq.filter (fun e -> e.Name.LocalName = "global")
        |> Seq.tryFind (fun e -> e.Attribute("scope").Value = scope)

    /// Extract attributes that are not the attribute names and object types from the global log object
    let private extractAttributesFromGlobalLog (log: XElement) =
        match log |> globalWithScope "log" with
        | None -> Map.empty
        | Some g ->
            g.Elements()
            |> Seq.filter (fun e ->
                getKeyAttributeWithKeyValue "attribute-names" e = None && 
                getKeyAttributeWithKeyValue "object-types" e = None)
            |> Seq.map (fun e ->
                match e.Attribute "key" |> Option.ofObj with
                | None -> failwith $"Property {e} has no key attribute."
                | Some key ->
                    match e.Attribute "value" |> Option.ofObj with
                    | None -> failwith $"Property {e} has no value attribute."
                    | Some value -> key.Value, extractValueFromAttribute value)
            |> Map.ofSeq

    /// Extract information from an object, given some extractor function
    let private extractFromObject (log: XElement) (name: string) extractor =
        match log.Elements() |> Seq.tryFind (fun e -> e.Name.LocalName = name) with
        | None -> failwith $"No <{name}> element defined for element: {log}"
        | Some root ->
            root.Elements()
            |> Seq.map(fun e -> getStringValueOfElementWithKey "id" e, extractor e)
            |> Map.ofSeq

    /// Extract all events from an OCEL log
    let private extractEvents (log: XElement) =
        let extractor (event: XElement) = {
            Activity = getStringValueOfElementWithKey "activity" event
            Timestamp = match getStringValueOfElementWithKey "timestamp" event |> System.DateTimeOffset.TryParse with
                        | true, dto -> dto
                        | _ -> failwith $"\"timestamp\" element cannot be parsed as DateTimeOffset on event: {event}"
            OMap = extractArray event "omap" (fun elem -> 
                match elem.Attribute "value" |> Option.ofObj with
                | Some v -> v.Value
                | None -> failwith $"No \"value\" attribute defined for element: {elem}")
            VMap = extractArray event "vmap" (fun elem ->
                match (elem.Attribute "key" |> Option.ofObj, elem.Attribute "value" |> Option.ofObj) with
                | Some k, Some v -> k.Value, extractValueFromAttribute v
                | None, Some _ -> failwith $"No \"key\" attribute defined for element: {elem}"
                | Some _, None -> failwith $"No \"value\" attribute defined for element: {elem}"
                | _ -> failwith $"No \"key\" and \"value\" attribute defined for element: {elem}")
                |> Map.ofSeq
        }
        extractFromObject log "events" extractor

    /// Extract all objects from an OCEL log
    let private extractObjects (log: XElement) =
        Map.empty

    /// Validate a XML string against the OCEL XML schema, with error messages
    let private validateXDocumentWithErrorMessages (xDoc: XDocument) =
        let schemaSet = new XmlSchemaSet()
        schemaSet.Add Schema |> ignore
        let mutable errors = ([]: string list)
        xDoc.Validate(schemaSet, validationEventHandler = ValidationEventHandler(fun _ args -> errors <- (args.Message :: errors)))
        match errors with
        | [] -> true, ([]: string seq)
        | _ -> false, errors |> Seq.ofList |> Seq.rev

    (* --- PUBLIC MEMBERS --- *)

    /// <inheritdoc />
    let validateWithErrorMessages (xml: string) =
        let xDoc = XDocument.Parse xml
        xDoc |> validateXDocumentWithErrorMessages
        
    /// <inheritdoc />
    let validate (xml: string) =
        xml |> validateWithErrorMessages |> fst

    /// <inheritdoc />
    let deserialize (xml: string) =
        let xDoc = XDocument.Parse xml
        match xDoc.Elements() |> Seq.tryFind (fun e -> e.Name.LocalName = "log") with
        | None -> failwith ""
        | Some xElm ->
            match xDoc |> validateXDocumentWithErrorMessages with
            | false, errors -> failwith $"""XML not validated by schema. Errors:{Environment.NewLine}{errors |> String.concat ", "}."""
            | true, _ ->
                {
                    GlobalAttributes = extractAttributesFromGlobalLog xElm
                    Events = extractEvents xElm
                    Objects = extractObjects xElm
                }
        
    /// <inheritdoc />
    let serialize (formatting: Formatting) (log: OcelLog) =
        ""