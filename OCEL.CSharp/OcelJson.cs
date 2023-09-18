using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OCEL.CSharp
{
    public static class OcelJson
    {
        /// <summary>
        /// Validate a JSON string against the OCEL JSON schema, with error messages.
        /// </summary>
        public static Tuple<bool, IEnumerable<string>> ValidateWithErrorMessages(string json)
        {
            return OCEL.OcelJson.validateWithErrorMessages(json);
        }

        /// <summary>
        /// Validate a JSON string against the OCEL JSON schema.
        /// </summary>
        public static bool Validate(string json)
        {
            return OCEL.OcelJson.validate(json);
        }

        /// <summary>
        /// Deserialize a JSON string into an OCEL log, and validate it against the OCEL schema.
        /// </summary>
        public static OcelLog Deserialize(string json, bool validate)
        {
            return OCEL.OcelJson.deserialize(validate, json).FromFSharpOcelLog();
        }

        /// <summary>
        /// Serialize an OCEL log into a JSON string.
        /// </summary>
        public static string Serialize(OcelLog log, Types.Formatting formatting, bool validate)
        {
            return OCEL.OcelJson.serialize(formatting, validate, log.ToFSharpOcelLog());
        }
    }
}
