namespace OCEL

open System
open System.Xml.Schema
open System.Xml.Linq
open FSharp.Data
open OCEL.Types

module Xml =
    type XmlOcelProvider = XmlProvider<Schema="../Schemas/schema.xml">

    /// Try to find the global element with a given scope
    let private globalWithScope scope (xml: XmlOcelProvider.Log) =
        xml.Globals |> Array.tryFind (fun e -> e.XElement.Attribute("scope").Value = scope)

    /// Extract the OCEL value from an attribute and try to find the correct type for it
    let private extractValueFromAttribute (xAttr: XAttribute) =
        match System.Int32.TryParse xAttr.Value with
        | true, int -> OcelInteger int
        | _ ->
            match System.Double.TryParse xAttr.Value with
            | true, double -> OcelFloat double
            | _ ->
                match System.Boolean.TryParse xAttr.Value with
                | true, bool -> OcelBoolean bool
                | _ ->
                    match System.DateTimeOffset.TryParse xAttr.Value with
                    | true, dto -> OcelTimestamp dto
                    | _ -> OcelString xAttr.Value
                    
    /// Get the attribute of an element with a given name
    let private getAttributeWithName name (xEl: XElement) =
        xEl.Attributes() |> Seq.tryFind (fun a -> a.Name.LocalName = name)

    /// Get the key attribute of an element where the value of the key matches the input
    let private getKeyAttributeWithKeyValye name (xEl: XElement) =
        match getAttributeWithName "key" xEl with
        | Some attr ->
            match attr.Value = name with
            | true -> Some attr
            | _ -> None
        | None -> None

    /// Extract attributes that are not the attribute names and object types from the global log object
    let private extractAttributesFromGlobalLog (log: XmlOcelProvider.Log) =
        match log |> globalWithScope "log" with
        | None -> Map.empty
        | Some g ->
            g.XElement.Elements()
            |> Seq.filter (fun e ->
                getKeyAttributeWithKeyValye "attribute-names" e = None && 
                getKeyAttributeWithKeyValye "object-types" e = None)
            |> Seq.map (fun e ->
                match getAttributeWithName "key" e with
                | None -> failwith $"Property {e} has no key attribute."
                | Some key ->
                    match getAttributeWithName "value" e with
                    | None -> failwith $"Property {e} has no value attribute."
                    | Some value -> key.Value, extractValueFromAttribute value)
            |> Map.ofSeq

    /// Validate a XML string against the OCEL XML schema, with error messages
    let private validateXDocumentWithErrorMessages (xDoc: XDocument) =
        let schema = XmlOcelProvider.GetSchema()
        let mutable errors = ([]: string list)
        xDoc.Validate(schema, validationEventHandler =
            ValidationEventHandler(fun _ args -> errors <- (args.Message :: errors)))
        match errors with
        | [] -> true, ([]: string seq)
        | _ -> false, errors |> Seq.ofList |> Seq.rev

    (* --- PUBLIC MEMBERS --- *)

    /// <inheritdoc />
    let validateWithErrorMessages (xml: string) =
        let parsed = XmlOcelProvider.Parse xml
        parsed.XElement.Document |> validateXDocumentWithErrorMessages
        
    /// <inheritdoc />
    let validate (xml: string) =
        xml |> validateWithErrorMessages |> fst

    /// <inheritdoc />
    let deserialize (xml: string) =
        let parsed = XmlOcelProvider.Parse xml
        match parsed.XElement.Document |> validateXDocumentWithErrorMessages with
        | false, errors -> failwith $"""XML not validated by schema. Errors:{Environment.NewLine}{errors |> String.concat ", "}."""
        | true, _ ->
            {
                GlobalAttributes = extractAttributesFromGlobalLog parsed
                Events = Map.empty
                Objects = Map.empty
            }

    /// <inheritdoc />
    let serialize (formatting: Formatting) (log: OcelLog) =
        ""