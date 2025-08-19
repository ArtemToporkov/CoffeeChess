using System.Text.Json.Serialization;
using CoffeeChess.Domain.Games.Enums;

namespace CoffeeChess.Domain.Matchmaking.ValueObjects;

[method: JsonConstructor]
public readonly struct ChallengeSettings(
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