namespace OCEL

open OCEL.Types

module Json =
    val validateWithErrorMessages : string -> bool * seq<string>
    val validate : string -> bool
    val deserialize : string -> OcelLog
    val serialize : Formatting -> OcelLog -> string