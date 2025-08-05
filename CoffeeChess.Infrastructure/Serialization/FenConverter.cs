using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Infrastructure.Serialization;

public class FenConverter : JsonConverter<Fen>
{
    public override Fen Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetString() ?? throw new SerializationException(
            $"Can't deserialize {nameof(Fen)} because it's not set to an instance of an object."));

    public override void Write(Utf8JsonWriter writer, Fen value, JsonSerializerOptions options)
        => writer.WriteStringValue(value);
}