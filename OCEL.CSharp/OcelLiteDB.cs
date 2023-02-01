using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiteDB;

namespace OCEL.CSharp
{
    // ReSharper disable once InconsistentNaming
    public static class OcelLiteDB
    {
        /// <summary>
        /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard, with error messages.
        /// </summary>
        public static Tuple<bool, IEnumerable<string>> ValidateWithErrorMessages(ILiteDatabase db)
        {
            return OCEL.OcelLiteDB.validateWithErrorMessage(db);
        }

        /// <summary>
        /// Validate a LiteDatabase in terms of its structure and content against the OCEL standard.
        /// </summary>
        public static bool Validate(ILiteDatabase db)
        {
            return OCEL.OcelLiteDB.validate(db);
        }

        /// <summary>
        /// Deserialize a LiteDatabase to an OCEL log.
        /// </summary>
        public static OcelLog Deserialize(ILiteDatabase db)
        {
            return OCEL.OcelLiteDB.deserialize(db).FromFSharpOcelLog();
        }

        /// <summary>
        /// Serialize an OCEL log into a LiteDatabase. Existing data is preserved and appended to.
        /// </summary>
        public static void Serialize(ILiteDatabase db, OcelLog log)
        {
            OCEL.OcelLiteDB.serialize(db, log.ToFSharpOcelLog());
        }
    }
}
