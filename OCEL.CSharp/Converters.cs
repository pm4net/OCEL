namespace OCEL.CSharp
{
    internal static class FSharpConverters
    {
        internal static IDictionary<TKey, TValue> ToExplicitDictionary<TKey, TValue>(
            this IDictionary<TKey, TValue> map) where TKey : notnull
        {
            return new Dictionary<TKey, TValue>(map);
        }

        internal static Microsoft.FSharp.Collections.FSharpMap<TKey, TValue> ToFSharpMap<TKey, TValue>(
            this IDictionary<TKey, TValue> dict) where TKey : notnull
        {
            return new Microsoft.FSharp.Collections.FSharpMap<TKey, TValue>(
                dict.Select(x => new Tuple<TKey, TValue>(x.Key, x.Value)));
        }

        internal static OcelLog FromFSharpOcelLog(this Types.OcelLog log)
        {
            return new OcelLog(log);
        }

        internal static Types.OcelLog ToFSharpOcelLog(this OcelLog log)
        {
            return new Types.OcelLog(
                globalAttributes: log.GlobalAttributes.ToFSharpMap(), 
                events: log.Events.ToDictionary(k => k.Key, v => new Types.OcelEvent(
                    v.Value.Activity, v.Value.Timestamp, v.Value.OMap, v.Value.VMap.ToFSharpMap()))
                    .ToFSharpMap(), 
                objects: log.Objects.ToDictionary(k => k.Key, v => new Types.OcelObject(
                        v.Value.Type, v.Value.OvMap.ToFSharpMap()))
                    .ToFSharpMap());
        }
    }
}
