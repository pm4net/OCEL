namespace OCEL

open OCEL.Types

module Xml =
    [<Literal>]
    let private SchemaXml = ""

    let validateWithErrorMessages (xml: string) =
        true, ([]: seq<string>)

    let validate (xml: string) =
        true

    let deserialize (xml: string) =
        {
            GlobalAttributes = Map.empty
            Events = Map.empty
            Objects = Map.empty
        }

    let serialize (log: OcelLog) =
        ""