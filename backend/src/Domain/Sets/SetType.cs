using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Common;

namespace Domain.Sets;

public static class SetTypeValues
{
    public const string Language = "language";
    public const string Subject = "subject";
}

[JsonConverter(typeof(SetTypeJsonConverter))]
public class SetType : ValueObject
{
    public string Value { get; }

    private SetType(string value)
    {
        Value = value;
    }

    public static SetType Language => new(SetTypeValues.Language);
    public static SetType Subject => new(SetTypeValues.Subject);

    public static SetType From(string value)
    {
        var type = AllowedTypes.FirstOrDefault(t =>
            string.Equals(t.Value, value, StringComparison.OrdinalIgnoreCase));

        if (type is null)
        {
            throw new ArgumentException($"Invalid set type: {value}");
        }

        return type;
    }

    public static bool TryParse(string? value, out SetType? result)
    {
        result = AllowedTypes.FirstOrDefault(t =>
            string.Equals(t.Value, value, StringComparison.OrdinalIgnoreCase));

        return result is not null;
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static IEnumerable<SetType> AllowedTypes
    {
        get
        {
            yield return Language;
            yield return Subject;
        }
    }
}

public class SetTypeJsonConverter : JsonConverter<SetType>
{
    public override SetType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.GetString() is { } str ? SetType.From(str) : null;

    public override void Write(Utf8JsonWriter writer, SetType value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
