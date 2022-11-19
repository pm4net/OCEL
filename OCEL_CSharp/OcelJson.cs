using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace OCEL
{
    public class OcelJson : IOcelHandler
    {
        private static readonly JSchema Schema = JSchema.Load(new JsonTextReader(File.OpenText(@"..\..\..\..\Schemas\schema.json")));

        public static OcelLog? Deserialize(string json)
        {
            if (!Validate(json)) return null;

            var jObj = JObject.Parse(json);

            var globalLog = ExtractGlobalLog(jObj);
            if (globalLog is null) return null;

            var jEvents = jObj["ocel:events"];
            if (jEvents is null) return null;
            var events = jEvents.Select(ExtractEvent);

            var jObjects = jObj["ocel:objects"];
            if (jObjects is null) return null;

            return new OcelLog(globalLog, null, null, null, null);
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

        /* --- HELPER METHODS --- */

        private static Value ExtractValue(JProperty jProperty)
        {
            return jProperty.First?.Type switch
            {
                JTokenType.Integer => new IntegerValue(jProperty.First!.Value<long>()),
                JTokenType.Float => new FloatValue(jProperty.First!.Value<double>()),
                JTokenType.String => new StringValue(jProperty.First!.Value<string>() ?? string.Empty),
                JTokenType.Boolean => new BooleanValue(jProperty.First!.Value<bool>()),
                JTokenType.Date => new TimestampValue(jProperty.First!.Value<DateTimeOffset>()),
                _ => throw new ArgumentOutOfRangeException(nameof(jProperty), $"Type {jProperty.Value.Type} not supported.")
            };
        }

        private static GlobalLog? ExtractGlobalLog(JObject jObj)
        {
            var jGlobalLog = jObj["ocel:global-log"];
            if (jGlobalLog is null) return null;

            var attributeNames = new List<string>();
            var objectTypes = new List<string>();
            var attributes = new Dictionary<string, Value>();

            foreach (var jProperty in jGlobalLog.Children<JProperty>())
            {
                switch (jProperty.Name)
                {
                    case "ocel:attribute-names" when jProperty.First?.Type == JTokenType.Array:
                        attributeNames = jProperty.First.Select(x => x.Value<string>()).ToList()!;
                        break;
                    case "ocel:object-types" when jProperty.First?.Type == JTokenType.Array:
                        objectTypes = jProperty.First.Select(x => x.Value<string>()).ToList()!;
                        break;
                    default:
                        attributes[jProperty.Name] = ExtractValue(jProperty);
                        break;
                }
            }

            return new GlobalLog(attributeNames, objectTypes, attributes);
        }

        private static Event? ExtractEvent(JToken jProperty)
        {
            return null;
        }

        private static Object? ExtractObject(JToken jProperty)
        {
            return null;
        }
    }
}
