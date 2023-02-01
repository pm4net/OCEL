namespace OCEL

open LiteDB
open OCEL.Types

module OcelLiteDB =
    /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard, with error messages.
    val validateWithErrorMessage : ILiteDatabase -> bool * seq<string>
    /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard.
    val validate : ILiteDatabase -> bool
    /// Deserialize a LiteDatabase to an OCEL log.
    val deserialize : ILiteDatabase -> OcelLog
    /// Serialize an OCEL log into a LiteDatabase. Existing data is preserved and appended to.
    val serialize : ILiteDatabase -> OcelLog -> unit