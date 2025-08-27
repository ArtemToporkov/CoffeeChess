using System.Text.Json;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Infrastructure.Serialization;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class RedisChallengeRepository(
    IConnectionMultiplexer redis) : IChallengeRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string ChallengeKeyPrefix = "challenge";
    
    public async Task<Challenge?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var hashEntries = await _database.HashGetAllAsync($"{ChallengeKeyPrefix}:{id}");
        if (hashEntries.Length == 0)
            return null;

        return GetChallengeFromHashEntriesOrThrow(hashEntries);
    }

    public async Task AddAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        var key = $"{ChallengeKeyPrefix}:{challenge.PlayerId}";
        
        // TODO: throw exception if the challenge already exists

        var hashEntries = GetHashEntriesFromChallenge(challenge);
        await _database.HashSetAsync(key, hashEntries);
    }

    public async Task DeleteAsync(Challenge chat, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{ChallengeKeyPrefix}:{chat.PlayerId}");

    public async Task SaveChangesAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        var key = $"{ChallengeKeyPrefix}:{challenge.PlayerId}";
        var hashEntries = GetHashEntriesFromChallenge(challenge);
        await _database.HashSetAsync(key, hashEntries);
    }

    public IEnumerable<Challenge> GetAll()
    {
        // TODO: don't use server.Keys because it freezes the server, use SCAN instead or implement FirstOrDefaultAsync
        
        var server = _database.Multiplexer
            .GetServer(_database.Multiplexer.GetEndPoints().First());

        foreach (var key in server.Keys(pattern: $"{ChallengeKeyPrefix}:*"))
        {
            var hashEntries = _database.HashGetAll(key);
            if (hashEntries.Length > 0)
                yield return GetChallengeFromHashEntriesOrThrow(hashEntries);
        }
    }

    private static HashEntry[] GetHashEntriesFromChallenge(Challenge challenge) =>
    [
        new("playerId", challenge.PlayerId),
        new("playerRating", challenge.PlayerRating),
        new("colorPreference", (int)challenge.ChallengeSettings.ColorPreference),
        new("timeControlMinutes", challenge.ChallengeSettings.TimeControl.Minutes),
        new("timeControlIncrement", challenge.ChallengeSettings.TimeControl.Increment),
        new("eloRatingPreferenceMin", challenge.ChallengeSettings.EloRatingPreference.Min),
        new("eloRatingPreferenceMax", challenge.ChallengeSettings.EloRatingPreference.Max)
    ];

    private static Challenge GetChallengeFromHashEntriesOrThrow(HashEntry[] hashEntries)
    {
        var hashTable = hashEntries.ToDictionary(
            entry => entry.Name.ToString(), entry => entry.Value);
        var playerId = hashTable["playerId"].ToString();
        var playerRating = ParseIntFromRedisValueOrThrow(hashTable["playerRating"]);
        var minRating = ParseIntFromRedisValueOrThrow(hashTable["eloRatingPreferenceMin"]);
        var maxRating = ParseIntFromRedisValueOrThrow(hashTable["eloRatingPreferenceMax"]);
        var minutes = ParseIntFromRedisValueOrThrow(hashTable["timeControlMinutes"]);
        var increment = ParseIntFromRedisValueOrThrow(hashTable["timeControlIncrement"]);
        var intColorPreference = ParseIntFromRedisValueOrThrow(hashTable["colorPreference"]);
        if (!Enum.IsDefined(typeof(ColorPreference), intColorPreference))
            throw new FormatException(
                $"Value \"{hashTable["colorPreference"]}\" can't be converted to ColorPreference.");
        // TODO: forbid usage of empty  domain structs ctors
        var timeControl = new TimeControl(minutes, increment);
        var ratingPreference = new EloRatingPreference(minRating, maxRating);
        var challengeSettings = new ChallengeSettings(timeControl, (ColorPreference)intColorPreference, ratingPreference);
        return new Challenge(playerId, playerRating, challengeSettings);
    }

    private static int ParseIntFromRedisValueOrThrow(RedisValue value)
    {
        if (!int.TryParse(value, out var result))
            throw new FormatException($"Value \"{value}\" can't be converted to int.");
        return result;
    }
}