using System.Text.Json.Serialization;
using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.ValueObjects;

[method: JsonConstructor]
public readonly struct GameSettings(
    int minutes,
    int increment,
    ColorPreference colorPreference = ColorPreference.Any,
    int minRating = 0,
    int maxRating = int.MaxValue)
{
    public int Minutes { get; } = minutes;
    public int Increment { get; } = increment;
    public ColorPreference ColorPreference { get; } = colorPreference;
    public int MinRating { get; } = minRating;
    public int MaxRating { get; } = maxRating;
}