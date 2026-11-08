using System.Text.Json;
using System.Text.Json.Serialization;

namespace eBike.API.Converters;

public class DateTimeUtcJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}

public class NullableDateTimeUtcJsonConverter : JsonConverter<DateTime?>
{
    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        return DateTime.SpecifyKind(reader.GetDateTime(), DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (!value.HasValue)
        {
            writer.WriteNullValue();
            return;
        }
        writer.WriteStringValue(DateTime.SpecifyKind(value.Value, DateTimeKind.Utc).ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));
    }
}
