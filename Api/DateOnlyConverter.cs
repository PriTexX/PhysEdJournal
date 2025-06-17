using System.Text.Json;
using System.Text.Json.Serialization;

namespace Api;

// Как-то получилось, что фронт привязался на такой всратый формат даты,
// так что теперь его нужно поддерживать и конвертировать в разные форматы на вход и на выход.
public class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            ArgumentNullException.ThrowIfNull(str);

            return DateOnly.ParseExact(str, "dd.MM.yyyy");
        }

        throw new JsonException("Unable to parse DateOnly from JSON.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("MM/dd/yyyy"));
    }
}
