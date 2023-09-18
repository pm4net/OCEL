using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCEL.CSharp
{
    public static class OcelXml
    {
        /// <summary>
        /// Validate a XML string against the OCEL XML schema, with error messages.
        /// </summary>
        public static Tuple<bool, IEnumerable<string>> ValidateWithErrorMessages(string xml)
        {
            return OCEL.OcelXml.validateWithErrorMessages(xml);
        }

        /// <summary>
        /// Validate a XML string against the OCEL XML schema.
        /// </summary>
        public static bool Validate(string xml)
        {
            return OCEL.OcelXml.validate(xml);
        }

        /// <summary>
        /// Deserialize a XML string into an OCEL log, and validate it against the OCEL schema.
        /// </summary>
        public static OcelLog Deserialize(string xml, bool validate)
        {
            return OCEL.OcelXml.deserialize(validate, xml).FromFSharpOcelLog();
        }

        /// <summary>
        /// Serialize an OCEL log into a XML string.
        /// </summary>
        public static string Serialize(OcelLog log, Types.Formatting formatting, bool validate)
        {
            return OCEL.OcelXml.serialize(formatting, validate, log.ToFSharpOcelLog());
        }
    }
}
