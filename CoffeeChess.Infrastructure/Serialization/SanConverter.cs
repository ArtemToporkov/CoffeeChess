using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Infrastructure.Serialization;

public class SanConverter : JsonConverter<San>
{
    public override San Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) 
        => new(reader.GetString() ?? throw new SerializationException(
            $"Can't deserialize {nameof(San)} because it's not set to an instance of an object."));

    public override void Write(Utf8JsonWriter writer, San value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}