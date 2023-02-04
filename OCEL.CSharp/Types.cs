using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OCEL;
using OCEL.Types;
using System.Linq;

namespace OCEL.CSharp
{
    public class OcelLog : IEquatable<OcelLog>
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
                Events.SelectMany(e => ExtractKeysFromMapping(e.Value.VMap)).Union(
                    Objects.SelectMany(o => ExtractKeysFromMapping(o.Value.OvMap))));

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

        /// <summary>
        /// An empty log
        /// </summary>
        public static OcelLog Empty => new OcelLog(new Dictionary<string, OcelValue>(), new Dictionary<string, OcelEvent>(), new Dictionary<string, OcelObject>());

        /// <summary>
        /// Merge a log with another log. Duplicate keys are overwritten by the other log.
        /// </summary>
        public OcelLog MergeWith(OcelLog other)
        {
            return new OcelLog(
                this.GlobalAttributes
                    .Concat(other.GlobalAttributes.Where(x => !this.GlobalAttributes.Keys.Contains(x.Key)))
                    .ToDictionary(x => x.Key, x => x.Value),
                this.Events
                    .Concat(other.Events.Where(x => !this.Events.Keys.Contains(x.Key)))
                    .ToDictionary(x => x.Key, x => x.Value),
                this.Objects
                    .Concat(other.Objects.Where(x => !this.Objects.Keys.Contains(x.Key)))                                         
                    .ToDictionary(x => x.Key, x => x.Value));
        }

        /// <summary>
        /// Merge duplicate objects and update the object ID's on the events that reference them.
        /// This is useful when the same object is repeatedly added without the ability to detect it efficiently, such as in logging.
        /// </summary>
        /// <returns>A new log instance with the duplicates removed and references updated.</returns>
        public OcelLog MergeDuplicateObjects()
        {
            // Converts log to F# representation and uses the method there, since the C# implementation uses reference equality for OCEL objects.
            // This makes it much easier to compare objects, even if nested, using built-in equality checking.
            return new OcelLog(this.ToFSharpOcelLog().MergeDuplicateObjects());
        }

        private static IEnumerable<string> ExtractKeysFromValue(OcelValue value)
        {
            switch (value)
            {
                case OcelList l:
                    return l.Values.SelectMany(ExtractKeysFromValue);
                case OcelMap m:
                    return ExtractKeysFromMapping(m.Values);
                default:
                    return new List<string>();
            }
        }

        private static IEnumerable<string> ExtractKeysFromMapping(IDictionary<string, OcelValue> mapping)
        {
            return mapping.SelectMany(x => ExtractKeysFromValue(x.Value).Append(x.Key));
        }

        /* --- EQUALITY MEMBERS --- */

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.ToFSharpOcelLog().IsEqual(((OcelLog)obj).ToFSharpOcelLog());
        }

        public bool Equals(OcelLog other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.ToFSharpOcelLog().IsEqual(other.ToFSharpOcelLog());
        }

        public static bool operator ==(OcelLog left, OcelLog right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OcelLog left, OcelLog right)
        {
            return !Equals(left, right);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (GlobalAttributes != null ? GlobalAttributes.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Events != null ? Events.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Objects != null ? Objects.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class OcelEvent : IEquatable<OcelEvent>
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

        /* --- EQUALITY MEMBERS --- */

        public bool Equals(OcelEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.ToFSharpOcelEvent().IsEqual(other.ToFSharpOcelEvent());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OcelEvent)obj);
        }

        public static bool operator ==(OcelEvent left, OcelEvent right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OcelEvent left, OcelEvent right)
        {
            return !Equals(left, right);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Activity != null ? Activity.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (OMap != null ? OMap.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (VMap != null ? VMap.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class OcelObject : IEquatable<OcelObject>
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

        /* --- EQUALITY MEMBERS --- */

        public bool Equals(OcelObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.ToFSharpOcelObject().IsEqual(other.ToFSharpOcelObject());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OcelObject)obj);
        }

        public static bool operator ==(OcelObject left, OcelObject right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OcelObject left, OcelObject right)
        {
            return !Equals(left, right);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (OvMap != null ? OvMap.GetHashCode() : 0);
            }
        }
    }

    public abstract class OcelValue : IEquatable<OcelValue>
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
                case Types.OcelValue.OcelList l:
                    return new OcelList(l.Item.Select(x => (OcelValue) x));
                case Types.OcelValue.OcelMap m:
                    return new OcelMap(m.Item.ToDictionary(x => x.Key, x => (OcelValue) x.Value));
                default:
                    throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        /* --- EQUALITY MEMBERS --- */

        public bool Equals(OcelValue other)
        {
            return this.ToFSharpOcelValue().IsEqual(other.ToFSharpOcelValue());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((OcelValue)obj);
        }

        public static bool operator ==(OcelValue left, OcelValue right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(OcelValue left, OcelValue right)
        {
            return !Equals(left, right);
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

    public class OcelList : OcelValue
    {
        public OcelList(IEnumerable<OcelValue> values)
        {
            Values = values;
        }

        public IEnumerable<OcelValue> Values { get; set; }
    }

    public class OcelMap : OcelValue
    {
        public OcelMap(IDictionary<string, OcelValue> values)
        {
            Values = values;
        }

        public IDictionary<string, OcelValue> Values { get; set; }
    }
}