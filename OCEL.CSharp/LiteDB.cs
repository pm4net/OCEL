using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace OCEL.CSharp
{
    public static class LiteDB
    {
        /// <summary>
        /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard, with error messages.
        /// </summary>
        public static Tuple<bool, IEnumerable<string>> ValidateWithErrorMessages(LiteDatabase db)
        {
            return OCEL.LiteDB.validateWithErrorMessage(db);
        }

        /// <summary>
        /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard.
        /// </summary>
        public static bool Validate(LiteDatabase db)
        {
            return OCEL.LiteDB.validate(db);
        }

        /// <summary>
        /// Deserialize a LiteDatabase to an OCEL log.
        /// </summary>
        public static OcelLog Deserialize(LiteDatabase db)
        {
            return OCEL.LiteDB.deserialize(db).FromFSharpOcelLog();
        }

        /// <summary>
        /// Serialize an OCEL log into a LiteDatabase. Existing data is preserved and appended to.
        /// </summary>
        public static void Serialize(LiteDatabase db, OcelLog log)
        {
            OCEL.LiteDB.serialize(db, log.ToFSharpOcelLog());
        }
    }
}
