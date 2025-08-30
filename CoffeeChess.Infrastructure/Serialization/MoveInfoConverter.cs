using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Infrastructure.Serialization;

public class MoveInfoConverter : JsonConverter<MoveInfo>
{
    public override MoveInfo Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var policy = options.PropertyNamingPolicy;
        var sanString = GetPropertyElementOrThrow(root, GetPropertyName(policy, nameof(MoveInfo.San)))
                           .GetString() 
                       ?? throw new JsonException($"Can't cast \"{nameof(MoveInfo.San)}\" property to string.");
        var san = new San(sanString);
        var timeAfterMoveElement = GetPropertyElementOrThrow(
            root, GetPropertyName(policy, nameof(MoveInfo.TimeAfterMove)));
        if (!timeAfterMoveElement.TryGetDouble(out var timeAfterMoveMicroseconds))
            throw new JsonException($"Can't get double from JsonElement \"{timeAfterMoveElement.ToString()}\".");
        var timeAfterMove = TimeSpan.FromMicroseconds(timeAfterMoveMicroseconds);
        return new(san, timeAfterMove);
    }

    public override void Write(Utf8JsonWriter writer, MoveInfo value, JsonSerializerOptions options)
    {
        var policy = options.PropertyNamingPolicy;
        writer.WriteStartObject();
        writer.WritePropertyName(GetPropertyName(policy, nameof(value.San)));
        JsonSerializer.Serialize(writer, value.San.ToString(), options);
        writer.WritePropertyName(GetPropertyName(policy, nameof(value.TimeAfterMove)));
        JsonSerializer.Serialize(writer, value.TimeAfterMove.TotalMicroseconds, options);
        writer.WriteEndObject();
    }
    
    private static string GetPropertyName(JsonNamingPolicy? policy, string propertyName) 
        => policy?.ConvertName(propertyName) ?? propertyName;

    private static JsonElement GetPropertyElementOrThrow(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property))
            throw new JsonException($"Property \"{propertyName}\" not found");
        return property;
    }
}