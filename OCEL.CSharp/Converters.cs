using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.FSharp.Collections;

namespace OCEL.CSharp
{
    public static class FSharpConverters
    {
        /// <summary>
        /// Create C# instance of an OCEL log from an F# OCEL log
        /// </summary>
        /// <param name="log">The F# OCEL log</param>
        /// <returns>A C# OCEL log</returns>
        public static OcelLog FromFSharpOcelLog(this Types.OcelLog log)
        {
            return new OcelLog(log);
        }

        /// <summary>
        /// Create a F# instance of an OCEL log from a C# OCEL log
        /// </summary>
        /// <param name="log">The C# OCEL log</param>
        /// <returns>A F# OCEL log</returns>
        public static Types.OcelLog ToFSharpOcelLog(this OcelLog log)
        {
            return new Types.OcelLog(
                globalAttributes: log.GlobalAttributes.ToFSharpMap(),
                events: log.Events.ToDictionary(
                        k => k.Key, 
                        v => new Types.OcelEvent(
                            v.Value.Activity, 
                            v.Value.Timestamp, 
                            ListModule.OfSeq(v.Value.OMap),
                            v.Value.VMap.ToFSharpMap()))
                    .ToFSharpMap(),
                objects: log.Objects.ToDictionary(
                    k => k.Key, 
                    v => new Types.OcelObject(
                        v.Value.Type, 
                        v.Value.OvMap.ToFSharpMap()))
                    .ToFSharpMap());
        }

        /// <summary>
        /// Create a C# instance of an OCEL event from a F# OCEL event
        /// </summary>
        public static OcelEvent FromFSharpOcelEvent(this Types.OcelEvent @event)
        {
            return new OcelEvent(@event.Activity, @event.Timestamp, @event.OMap, @event.VMap.ToDictionary(x => x.Key, x => x.Value.FromFSharpOcelValue()));
        }

        /// <summary>
        /// Create a F# instance of an OCEL event from a C# OCEL event
        /// </summary>
        public static Types.OcelEvent ToFSharpOcelEvent(this OcelEvent @event)
        {
            return new Types.OcelEvent(@event.Activity, @event.Timestamp, ListModule.OfSeq(@event.OMap), ToFSharpMap(@event.VMap));
        }

        /// <summary>
        /// Create a C# instance of an OCEL object from a F# OCEL object
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public static OcelObject FromFSharpOcelObject(this Types.OcelObject @object)
        {
            return new OcelObject(@object.Type, @object.OvMap.ToDictionary(x => x.Key, x => x.Value.FromFSharpOcelValue()));
        }

        /// <summary>
        /// Create a F# instance of an OCEL object from a C# OCEL object
        /// </summary>
        public static Types.OcelObject ToFSharpOcelObject(this OcelObject @object)
        {
            return new Types.OcelObject(@object.Type, ToFSharpMap(@object.OvMap));
        }

        /// <summary>
        /// Convert a F# OcelValue to the C# equivalent
        /// </summary>
        public static OcelValue FromFSharpOcelValue(this Types.OcelValue v)
        {
            switch (v)
            {
                case Types.OcelValue.OcelString ocelString:
                    return new OcelString(ocelString.Item);
                case Types.OcelValue.OcelTimestamp ocelTimestamp:
                    return new OcelTimestamp(ocelTimestamp.Item);
                case Types.OcelValue.OcelInteger ocelInteger:
                    return new OcelInteger(ocelInteger.Item);
                case Types.OcelValue.OcelFloat ocelFloat:
                    return new OcelFloat(ocelFloat.Item);
                case Types.OcelValue.OcelBoolean ocelBoolean:
                    return new OcelBoolean(ocelBoolean.Item);
                case Types.OcelValue.OcelList ocelList:
                    return new OcelList(ocelList.Item.Select(FromFSharpOcelValue));
                case Types.OcelValue.OcelMap ocelMap:
                    return new OcelMap(ocelMap.Item.ToDictionary(x => x.Key, x => x.Value.FromFSharpOcelValue()));
            }

            return v.IsOcelNull ? new OcelNull() : throw new ArgumentOutOfRangeException(nameof(v));
        }

        /// <summary>
        /// Convert a C# OcelValue to the F# equivalent
        /// </summary>
        public static Types.OcelValue ToFSharpOcelValue(this OcelValue v)
        {
            switch (v)
            {
                case OcelString s:
                    return Types.OcelValue.NewOcelString(s.Value);
                case OcelTimestamp t:
                    return Types.OcelValue.NewOcelTimestamp(t.Value);
                case OcelInteger i:
                    return Types.OcelValue.NewOcelInteger(i.Value);
                case OcelFloat f:
                    return Types.OcelValue.NewOcelFloat(f.Value);
                case OcelBoolean b:
                    return Types.OcelValue.NewOcelBoolean(b.Value);
                case OcelList l:
                    return Types.OcelValue.NewOcelList(ListModule.OfSeq(l.Values.Select(x => x.ToFSharpOcelValue())));
                case OcelMap m:
                    return Types.OcelValue.NewOcelMap(m.Values.ToFSharpMap());
                case OcelNull n:
                    return Types.OcelValue.OcelNull;
                default:
                    throw new ArgumentOutOfRangeException(nameof(v));
            }
        }

        /// <summary>
        /// Convert a generic dictionary to a generic F# map
        /// </summary>
        public static FSharpMap<TKey, TValue> ToFSharpMap<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            return new FSharpMap<TKey, TValue>(dict.Where(x => x.Value != null).Select(x => new Tuple<TKey, TValue>(x.Key, x.Value)));
        }

        /// <summary>
        /// Convert a dictionary with a string and OcelValue to an F# map where the OcelValue is mapped to the F# OcelValue
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static FSharpMap<string, Types.OcelValue> ToFSharpMap(this IDictionary<string, OcelValue> dict)
        {
            return dict.Where(x => x.Value != null).ToDictionary(x => x.Key, x => x.Value.ToFSharpOcelValue()).ToFSharpMap();
        }
    }
}
