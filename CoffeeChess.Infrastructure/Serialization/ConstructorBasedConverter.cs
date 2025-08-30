using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoffeeChess.Infrastructure.Serialization;

public class ConstructorBasedConverter<T> : JsonConverter<T>
{
    private readonly ConstructorInfo? _ctor;
    private readonly ParameterInfo[] _params;
    private readonly PropertyInfo[] _props;

    public ConstructorBasedConverter()
    {
        _ctor = typeof(T)
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();
        _params = _ctor?.GetParameters() ?? [];
        _props = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToArray();
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (_ctor == null || _params.Length == 0)
            return JsonSerializer.Deserialize<T>(ref reader, options)!;

        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var args = new object?[_params.Length];

        for (var i = 0; i < _params.Length; i++)
        {
            var parameter = _params[i];
            var jsonName = options.PropertyNamingPolicy?.ConvertName(parameter.Name!) ?? parameter.Name!;
            var elem = default(JsonElement);
            var found = root.ValueKind == JsonValueKind.Object && root.TryGetProperty(jsonName, out elem);

            if (!found && options.PropertyNameCaseInsensitive 
                       && root.ValueKind == JsonValueKind.Object
                       && TryGetPropertyValueIgnoreCase(root, jsonName, out var prop))
            {
                elem = prop.Value;
                found = true;
            }

            if (found)
                args[i] = elem.Deserialize(parameter.ParameterType, options);
            else
                args[i] = parameter.HasDefaultValue
                    ? parameter.DefaultValue
                    : parameter.ParameterType.IsValueType
                        ? Activator.CreateInstance(parameter.ParameterType)
                        : null;
        }

        return (T)_ctor.Invoke(args);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var property in _props)
        {
            var val = property.GetValue(value);
            var name = options.PropertyNamingPolicy?.ConvertName(property.Name) ?? property.Name;
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, val, property.PropertyType, options);
        }

        writer.WriteEndObject();
    }

    private static bool TryGetPropertyValueIgnoreCase(
        JsonElement jsonElement, string propertyName, [NotNullWhen(true)] out JsonElement? value)
    {
        foreach (var prop in jsonElement
                     .EnumerateObject()
                     .Where(jsonProperty
                         => string.Equals(jsonProperty.Name, propertyName, StringComparison.OrdinalIgnoreCase)))
        {
            value = prop.Value;
            return true;
        }
        
        value = null;
        return false;
    }
}