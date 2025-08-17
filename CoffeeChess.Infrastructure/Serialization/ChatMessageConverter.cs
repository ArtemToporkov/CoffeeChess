using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeeChess.Domain.Chats.ValueObjects;
namespace CoffeeChess.Infrastructure.Serialization;

public class ChatMessageConverter : JsonConverter<ChatMessage>
{
    public override ChatMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var policy = options.PropertyNamingPolicy;
        var username = GetPropertyElementOrThrow(root, GetPropertyName(policy, nameof(ChatMessage.Username)))
                           .GetString() 
                       ?? throw new JsonException($"Can't cast \"{nameof(ChatMessage.Username)}\" property to string.");
        var message = GetPropertyElementOrThrow(root, GetPropertyName(policy, nameof(ChatMessage.Message)))
                          .GetString() 
                      ?? throw new JsonException($"Can't cast \"{nameof(ChatMessage.Message)}\" property to string.");
        var timestampEl = GetPropertyElementOrThrow(
            root, GetPropertyName(policy, nameof(ChatMessage.Timestamp)));
        var timestamp = timestampEl.Deserialize<DateTime>();
        return new(username, message, timestamp);
    }

    public override void Write(Utf8JsonWriter writer, ChatMessage value, JsonSerializerOptions options)
    {
        var policy = options.PropertyNamingPolicy;
        writer.WriteStartObject();
        writer.WritePropertyName(GetPropertyName(policy, nameof(value.Username)));
        JsonSerializer.Serialize(writer, value.Username, options);
        writer.WritePropertyName(GetPropertyName(policy, nameof(value.Message)));
        JsonSerializer.Serialize(writer, value.Message, options);
        writer.WritePropertyName(GetPropertyName(policy, nameof(value.Timestamp)));
        JsonSerializer.Serialize(writer, value.Timestamp, options);
        writer.WriteEndObject();
    }
    
    private static string GetPropertyName(JsonNamingPolicy? policy, string propertyName) 
        => policy?.ConvertName(propertyName) ?? propertyName;

    private static JsonElement GetPropertyElementOrThrow(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var property))
            throw new JsonException($"Property \"{propertyName}\" not found");
        return property;
    }
}