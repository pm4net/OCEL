namespace OCEL

open OCEL.Types

module OcelXml =
    /// Validate a XML string against the OCEL XML schema, with error messages.
    val validateWithErrorMessages : string -> bool * seq<string>
    /// Validate a XML string against the OCEL XML schema.
    val validate : string -> bool
    /// Deserialize a XML string into an OCEL log, and validate it against the OCEL schema.
    val deserialize : bool -> string -> OcelLog
    /// Serialize an OCEL log into a JSON string.
    val serialize : Formatting -> OcelLog -> string