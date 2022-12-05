namespace OCEL

open LiteDB
open OCEL.Types

module LiteDB =
    /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard, with error messages.
    val validateWithErrorMessage : LiteDatabase -> bool * seq<string>
    /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard.
    val validate : LiteDatabase -> bool
    /// Deserialize a LiteDatabase to an OCEL log.
    val deserialize : LiteDatabase -> OcelLog
    /// Serialize an OCEL log into a LiteDatabase. Existing data is preserved and appended to.
    val serialize : LiteDatabase -> OcelLog -> unit