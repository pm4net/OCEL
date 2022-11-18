using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace OCEL
{
    internal class OcelJsonHandler : IOcelHandler
    {
        private static readonly JSchema Schema = JSchema.Load(new JsonTextReader(File.OpenText(@"..\Schemas\schema.json")));

        public static OcelLog? Deserialize(string json)
        {
            var reader = new JsonTextReader(new StringReader(json));
            var validatingReader = new JSchemaValidatingReader(reader) { Schema = Schema };
            var serializer = new JsonSerializer();
            var log = serializer.Deserialize<OcelLog>(validatingReader);
            return log;
        }

        public static string? Serialize(OcelLog log)
        {
            throw new NotImplementedException();
        }

        public static bool Validate(string json)
        {
            var parsed = JObject.Parse(json);
            return parsed.IsValid(Schema);
        }

        public static (bool, IEnumerable<string>) ValidateWithErrorMessages(string json)
        {
            var parsed = JObject.Parse(json);
            var valid = parsed.IsValid(Schema, out IList<string> errors);
            return (valid, errors);
        }
    }
}
