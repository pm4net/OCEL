namespace OCEL

open System.Xml.Schema
open FSharp.Data
open OCEL.Types

module Xml =
    type XmlOcelProvider = XmlProvider<Schema="../Schemas/schema.xml">

    let validateWithErrorMessages (xml: string) =
        true, ([]: seq<string>)

    let validate (xml: string) =
        let schema = XmlOcelProvider.GetSchema()
        let parsed = System.Xml.Linq.XDocument.Parse xml // Not using XmlOcelProvider as it fails outright if the schema doesn't match
        let mutable errors = ([]: string list) // todo has to be enumerable and use add function
        parsed.Validate(schema, validationEventHandler = ValidationEventHandler(fun obj args -> (args.Message :: errors) |> ignore))
        
        true

    let deserialize (xml: string) =
        let parsed = XmlOcelProvider.Parse xml
        {
            GlobalAttributes = Map.empty
            Events = Map.empty
            Objects = Map.empty
        }

    let serialize (formatting: Formatting) (log: OcelLog) =
        ""