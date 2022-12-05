namespace OCEL

open LiteDB
open OCEL.Types

module LiteDB =
    /// Validate a LiteDatabase according to the OCEL structure, with error messages.
    val validateWithErrorMessage : LiteDatabase -> bool * seq<string>
    /// Validate a LiteDatabase according to the OCEL structure.
    val validate : LiteDatabase -> bool
    /// Deserialize a LiteDatabase to an OCEL log.
    val deserialize : LiteDatabase -> OcelLog
    /// Serialize an OCEL log into a LiteDatabase.
    val serialize : LiteDatabase -> OcelLog -> LiteDatabase