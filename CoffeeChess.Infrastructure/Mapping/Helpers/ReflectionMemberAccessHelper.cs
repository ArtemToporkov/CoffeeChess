using System.Reflection;
using CoffeeChess.Domain.Games.AggregatesRoots;

namespace CoffeeChess.Infrastructure.Mapping.Helpers;

public static class ReflectionMemberAccessHelper
{
    public static FieldInfo GetPrivateFieldOrThrow<TType>(string fieldName)
        => typeof(TType).GetField(
               fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
           ?? throw new MissingFieldException(nameof(Game), fieldName);
    
    public static TOutValue GetFieldValueOrThrow<TType, TOutValue>(
        FieldInfo fieldInfo, 
        TType obj)
    {
        var value = fieldInfo.GetValue(obj);

        if (value is null)
            throw new InvalidCastException(
                $"Field \"{fieldInfo.Name}\" is null, can't cast to {typeof(TOutValue).FullName}.");

        if (value is TOutValue typedValue)
            return typedValue;

        throw new InvalidCastException(
            $"Can't cast field \"{fieldInfo.Name}\" " +
            $"of type {fieldInfo.FieldType.FullName} to {typeof(TOutValue).FullName}.");
    }

    public static void SetPropertyValueOrThrow<TType>(TType obj, string propertyName, object? propertyValue)
    {
        if (string.IsNullOrEmpty(propertyName)) 
            throw new ArgumentNullException(nameof(propertyName));

        var type = typeof(TType);
        var property = type.GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property == null)
            throw new Exception($"A property with a name {propertyName} not found in type {type.FullName}.");

        if (property.CanWrite)
        {
            property.SetValue(obj, propertyValue);
            return;
        }

        try
        {
            SetFieldValueOrThrow(obj, type, $"<{propertyName}>k__BackingField", propertyValue);
        }
        catch (MissingFieldException)
        {
            throw new Exception(
                $"Cannot set value for a property with a name {propertyName} — no setter or backing field found.");
        }
    }

    public static void SetFieldValueOrThrow<TType>(TType obj, string fieldName, object? fieldValue)
        => SetFieldValueOrThrow(obj, typeof(TType), fieldName, fieldValue);

    private static void SetFieldValueOrThrow<TType>(TType obj, Type type, string fieldName, object? fieldValue)
    {
        var field = GetBaseTypesAndSelf(type)
            .Select(t => t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic))
            .FirstOrDefault(f => f != null);

        if (field == null) 
            throw new MissingFieldException(type.FullName, fieldName);
        field.SetValue(obj, fieldValue);
    }
    
    private static IEnumerable<Type> GetBaseTypesAndSelf(Type type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }
}