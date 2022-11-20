using OCEL;
using OCEL.Types;
using System.Linq;

namespace OCEL.CSharp
{
    public class OcelLog
    {
        public OcelLog(
            IDictionary<string, OcelValue> globalAttributes, 
            IDictionary<string, OcelEvent> events, 
            IDictionary<string, OcelObject> objects)
        {
            GlobalAttributes = globalAttributes;
            Events = events;
            Objects = objects;
        }

        internal OcelLog(OCEL.Types.OcelLog log)
        {
            GlobalAttributes = Converters.ToStandardDictionary(log.GlobalAttributes);
            Events = Converters
                .ToStandardDictionary(log.Events)
                .ToDictionary(
                    k => k.Key, 
                    v => new OcelEvent(
                        v.Value.Activity, v.Value.Timestamp,
                        v.Value.OMap, Converters.ToStandardDictionary(v.Value.VMap)));
            Objects = null; // TODO
        }

        public IDictionary<string, OcelValue> GlobalAttributes { get; set; }

        public IDictionary<string, OcelEvent> Events { get; set; }

        public IDictionary<string, OcelObject> Objects { get; set; }

        public ISet<string> AttributeNames => new HashSet<string>();

        public ISet<string> ObjectTypes => new HashSet<string>();

        public bool IsValid => true;
    }

    public class OcelEvent
    {
        public OcelEvent(
            string activity, 
            DateTimeOffset timestamp, 
            IEnumerable<string> oMap, 
            IDictionary<string, OcelValue> vMap)
        {
            Activity = activity;
            Timestamp = timestamp;
            OMap = oMap;
            VMap = vMap;
        }

        public string Activity { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public IEnumerable<string> OMap { get; set; }

        public IDictionary<string, OcelValue> VMap { get; set; }
    }

    public class OcelObject
    {
        public string Type { get; set; }

        public IDictionary<string, OcelValue> OvMap { get; set; }
    }
}