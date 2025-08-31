using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Persistence.Models;

public class ChallengePersistenceModel
{
    public const string PlayerIdName = "playerId";
    public const string PlayerRatingName = "playerRating";
    public const string TimeControlMinutesName = "timeControlMinutes";
    public const string TimeControlIncrementName = "timeControlIncrement";
    public const string ColorPreferenceName = "colorPreference";
    public const string MinEloRatingPreferenceName = "minEloRatingPreference";
    public const string MaxEloRatingPreferenceName = "maxEloRatingPreference";
    
    public string PlayerId { get; private init; }
    public int PlayerRating { get; private init; }
    public int TimeControlMinutes { get; private init; }
    public int TimeControlIncrement { get; private init; }
    public int ColorPreference { get; private init; }
    public int MinEloRatingPreference { get; private init; }
    public int MaxEloRatingPreference { get; private init; }

    public static ChallengePersistenceModel FromChallenge(Challenge challenge)
        => new()
        {
            PlayerId = challenge.PlayerId,
            PlayerRating = challenge.PlayerRating,
            TimeControlMinutes = challenge.ChallengeSettings.TimeControl.Minutes,
            TimeControlIncrement = challenge.ChallengeSettings.TimeControl.Increment,
            ColorPreference = (int)challenge.ChallengeSettings.ColorPreference,
            MinEloRatingPreference = challenge.ChallengeSettings.EloRatingPreference.Min,
            MaxEloRatingPreference = challenge.ChallengeSettings.EloRatingPreference.Max,
        };

    public Challenge ToChallenge()
    {
        var eloRatingPreference = new EloRatingPreference(MinEloRatingPreference, MaxEloRatingPreference);
        var timeControl = new TimeControl(TimeControlMinutes, TimeControlIncrement);
        if (!Enum.IsDefined(typeof(ColorPreference), ColorPreference))
            throw new InvalidCastException(
                $"Can't cast {nameof(ColorPreference)} with value \"{ColorPreference}\" " +
                $"to {nameof(Domain.Matchmaking.Enums.ColorPreference)}");
        var colorPreference = (ColorPreference)ColorPreference;
        var challengeSettings = new ChallengeSettings(timeControl, colorPreference, eloRatingPreference);
        return new(PlayerId, PlayerRating, challengeSettings);
    }

    public HashEntry[] ToHashEntries()
        =>
        [
            new(PlayerIdName, PlayerId),
            new(PlayerRatingName, PlayerRating),
            new(ColorPreferenceName, ColorPreference),
            new(TimeControlMinutesName, TimeControlMinutes),
            new(TimeControlIncrementName, TimeControlIncrement),
            new(MinEloRatingPreferenceName, MinEloRatingPreference),
            new(MaxEloRatingPreferenceName, MaxEloRatingPreference),
        ];
    
    public static Challenge FromRedisResult(RedisResult[] resultEntries)
    {
        if (resultEntries.Length == 0 || resultEntries is null) 
            throw new ArgumentNullException(nameof(resultEntries));

        var dict = new Dictionary<string, RedisResult>();
        for (var i = 0; i < resultEntries.Length - 1; i += 2)
        {
            var key = (string?)resultEntries[i];
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Key can't be null.");
            
            var value = resultEntries[i + 1];
            dict[key] = value;
        }

        var model = new ChallengePersistenceModel
        {
            PlayerId = (string)(dict[PlayerIdName] ?? throw new ArgumentNullException(
                $"Value for key \"{PlayerIdName}\" is null."))!,
            PlayerRating = (int)dict[PlayerRatingName],
            TimeControlMinutes = (int)dict[TimeControlMinutesName],
            TimeControlIncrement = (int)dict[TimeControlIncrementName],
            ColorPreference = (int)dict[ColorPreferenceName],
            MinEloRatingPreference = (int)dict[MinEloRatingPreferenceName],
            MaxEloRatingPreference = (int)dict[MaxEloRatingPreferenceName]
        };

        return model.ToChallenge();
    }
}