namespace OCEL

open OCEL.Types

module Xml =
    [<Literal>]
    let private SchemaXml = ""

    let Validate xml =
        true

    let ValidateWithErrorMessages xml =
        true, []

    let Deserialize xml =
        []

    let Serialize log =
        ""