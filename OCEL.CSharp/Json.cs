using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCEL.CSharp
{
    public static class Json
    {
        /// <summary>
        /// Validate a JSON string against the OCEL JSON schema, with error messages.
        /// </summary>
        public static Tuple<bool, IEnumerable<string>> ValidateWithErrorMessages(string json)
        {
            return OCEL.Json.ValidateWithErrorMessages(json);
        }

        /// <summary>
        /// Validate a JSON string against the OCEL JSON schema.
        /// </summary>
        public static bool Validate(string json)
        {
            return OCEL.Json.Validate(json);
        }

        /// <summary>
        /// Deserialize a JSON string into an OCEL log, and validate it against the OCEL schema.
        /// </summary>
        public static OcelLog Deserialize(string json)
        {
            return OCEL.Json.Deserialize(json).FromFSharpOcelLog();
        }

        /// <summary>
        /// Serialize an OCEL log into a JSON string.
        /// </summary>
        public static string Serialize(OcelLog log)
        {
            return OCEL.Json.Serialize(log.ToFSharpOcelLog());
        }
    }
}
