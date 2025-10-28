using System;
using System.Globalization;
using Newtonsoft.Json;

namespace MatchesService.Json;

public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    private static readonly string[] AcceptedFormats =
    {
        "HH:mm:ss",
        "H:mm:ss",
        "HH:mm",
        "H:mm"
    };

    public override void WriteJson(JsonWriter writer, TimeOnly value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
    }

    public override TimeOnly ReadJson(JsonReader reader, Type objectType, TimeOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String && reader.Value is string text)
        {
            text = text.Trim();
            foreach (var format in AcceptedFormats)
            {
                if (TimeOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
                {
                    return value;
                }
            }

            if (TimeSpan.TryParse(text, CultureInfo.InvariantCulture, out var timeSpan))
            {
                return TimeOnly.FromTimeSpan(timeSpan);
            }

            throw new JsonSerializationException($"Formato de hora inválido: \"{text}\". Usa HH:mm o HH:mm:ss.");
        }

        throw new JsonSerializationException($"El token JSON {reader.TokenType} no es válido para un TimeOnly.");
    }
}
