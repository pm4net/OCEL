using System;
using System.Collections.Generic;
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

        internal OcelLog(Types.OcelLog log)
        {
            GlobalAttributes = log.GlobalAttributes.ToDictionary(x => x.Key, x => (OcelValue) x.Value);
            Events = log.Events.ToDictionary(k => k.Key, v => new OcelEvent(v.Value));
            Objects = log.Objects.ToDictionary(k => k.Key, v => new OcelObject(v.Value));
        }

        public IDictionary<string, OcelValue> GlobalAttributes { get; set; }

        public IDictionary<string, OcelEvent> Events { get; set; }

        public IDictionary<string, OcelObject> Objects { get; set; }

        /// <summary>
        /// The set of all attribute names used in both events and objects in the log
        /// </summary>
        public ISet<string> AttributeNames =>
            new HashSet<string>(
                Events.SelectMany(e => e.Value.VMap.Keys).Union(
                    Objects.SelectMany(o => o.Value.OvMap.Keys)));

        /// <summary>
        /// The set of all object types in the log
        /// </summary>
        public ISet<string> ObjectTypes => new HashSet<string>(Objects.Select(o => o.Value.Type));

        /// <summary>
        /// The list of events and their ID's, ordered by their timestamp
        /// </summary>
        public IEnumerable<KeyValuePair<string, OcelEvent>> OrderedEvents => Events.ToList().OrderBy(e => e.Value.Timestamp);

        /// <summary>
        /// Indicates whether the log is valid according to the OCEL specification
        /// </summary>
        public bool IsValid => Events.All(e => e.Value.OMap.All(o => Objects.ContainsKey(o)));
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

        internal OcelEvent(Types.OcelEvent ocelEvent)
        {
            Activity = ocelEvent.Activity;
            Timestamp = ocelEvent.Timestamp;
            OMap = ocelEvent.OMap;
            VMap = ocelEvent.VMap.ToDictionary(x => x.Key, x => (OcelValue) x.Value);
        }

        public string Activity { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public IEnumerable<string> OMap { get; set; }

        public IDictionary<string, OcelValue> VMap { get; set; }
    }

    public class OcelObject
    {
        public OcelObject(string type, IDictionary<string, OcelValue> ovMap)
        {
            Type = type;
            OvMap = ovMap;
        }

        internal OcelObject(Types.OcelObject ocelObject)
        {
            Type = ocelObject.Type;
            OvMap = ocelObject.OvMap.ToDictionary(x => x.Key, x => (OcelValue) x.Value);
        }

        public string Type { get; set; }

        public IDictionary<string, OcelValue> OvMap { get; set; }
    }

    public abstract class OcelValue
    {
        public static explicit operator OcelValue(Types.OcelValue v)
        {
            switch (v)
            {
                case Types.OcelValue.OcelString s:
                    return new OcelString(s.Item);
                case Types.OcelValue.OcelTimestamp t:
                    return new OcelTimestamp(t.Item);
                case Types.OcelValue.OcelInteger i:
                    return new OcelInteger(i.Item);
                case Types.OcelValue.OcelFloat f:
                    return new OcelFloat(f.Item);
                case Types.OcelValue.OcelBoolean b:
                    return new OcelBoolean(b.Item);
                default:
                    throw new ArgumentOutOfRangeException(nameof(v));
            }
        }
    }

    public class OcelString : OcelValue
    {
        public OcelString(string value)
        {
            Value = value;
        }

        public string Value { get; set; }
    }

    public class OcelTimestamp : OcelValue
    {
        public OcelTimestamp(DateTimeOffset value)
        {
            Value = value;
        }

        public DateTimeOffset Value { get; set; }
    }

    public class OcelInteger : OcelValue
    {
        public OcelInteger(long value)
        {
            Value = value;
        }

        public long Value { get; set; }
    }

    public class OcelFloat : OcelValue
    {
        public OcelFloat(double value)
        {
            Value = value;
        }

        public double Value { get; set; }
    }

    public class OcelBoolean : OcelValue
    {
        public OcelBoolean(bool value)
        {
            Value = value;
        }

        public bool Value { get; set; }
    }
}