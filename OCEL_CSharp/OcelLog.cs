namespace OCEL
{
    public abstract record Value;
    public record StringValue(string Value) : Value;
    public record TimestampValue(DateTimeOffset Value) : Value;
    public record IntegerValue(long Value) : Value;
    public record FloatValue(double Value) : Value;
    public record BooleanValue(bool Value) : Value;

    public record Event(
        string Id,
        string Activity,
        DateTimeOffset Timestamp,
        IList<string> ObjectIds,
        IDictionary<string, Value> Attributes);

    public record Object(
        string Id,
        string Type,
        IDictionary<string, Value> Attributes);

    public record GlobalLog(
        IList<string> AttributeNames,
        IList<string> ObjectTypes,
        IDictionary<string, Value> Attributes);

    public record OcelLog(
        GlobalLog GlobalLog, 
        Event GlobalEvent,
        Object GlobalObject,
        IList<Event> Events,
        IList<Object> Objects);
}
