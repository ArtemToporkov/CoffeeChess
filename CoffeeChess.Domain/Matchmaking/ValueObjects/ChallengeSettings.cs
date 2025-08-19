using CoffeeChess.Domain.Matchmaking.Enums;

namespace CoffeeChess.Domain.Matchmaking.ValueObjects;

public readonly struct ChallengeSettings(
    TimeControl timeControl,
    ColorPreference colorPreference = ColorPreference.Any,
    EloRatingPreference? eloRatingPreference = null)
{
    public TimeControl TimeControl { get; } = timeControl;
    public ColorPreference ColorPreference { get; } = colorPreference;
    public EloRatingPreference EloRatingPreference { get; } = eloRatingPreference ?? EloRatingPreference.Any;
}