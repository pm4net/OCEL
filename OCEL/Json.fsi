namespace OCEL

open OCEL.Types

module Json =
    /// Validate a JSON string against the OCEL JSON schema, with error messages.
    val validateWithErrorMessages : string -> bool * seq<string>
    /// Validate a JSON string against the OCEL JSON schema.
    val validate : string -> bool
    /// Deserialize a JSON string into an OCEL log, and validate it against the OCEL schema.
    val deserialize : string -> OcelLog
    /// Serialize an OCEL log into a JSON string.
    val serialize : Formatting -> OcelLog -> string