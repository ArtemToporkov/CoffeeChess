using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Exceptions;
using MediatR;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Services.Implementations;

public class RedisMatchmakingService(
    IMediator mediator,
    IConnectionMultiplexer redis,
    IPlayerRepository playerRepository) : IMatchmakingService
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string ChallengeKeyPrefix = "challenge";

    public async Task QueueOrFindChallenge(
        string playerId, ChallengeSettings settings, CancellationToken cancellationToken = default)
    {
        var playerRating = (await playerRepository.GetByIdAsync(playerId, cancellationToken))?.Rating
            ?? throw new NotFoundException(nameof(Player), playerId);
        var challenge = new Challenge(playerId, playerRating, settings);
        var matchingChallenge = await TryFindFirstMatchingChallengeAndRemove(challenge, cancellationToken);
        if (matchingChallenge is null)
            await AddAsync(challenge, cancellationToken);
        else
        {
            challenge.Accept(matchingChallenge);
            foreach (var @event in challenge.DomainEvents)
                await mediator.Publish(@event, cancellationToken);
            challenge.ClearDomainEvents();
        }
    }

    private async Task<Challenge?> TryFindFirstMatchingChallengeAndRemove(
        Challenge challenge, CancellationToken cancellationToken = default)
    {
        // TODO: implement matching challenge searching using Redis Sorted Set
        var matchingChallenge = GetAll()
            .FirstOrDefault(candidateChallenge => ValidatePlayerForChallenge(
                challenge.PlayerRating, challenge.ChallengeSettings, candidateChallenge));
        if (matchingChallenge is not null)
            await DeleteAsync(matchingChallenge, cancellationToken);
        return matchingChallenge;
    }

    private static bool ValidatePlayerForChallenge(
        int playerRating, ChallengeSettings playerSettings, Challenge challengeToJoin)
    {
        return playerSettings.TimeControl.Minutes == challengeToJoin.ChallengeSettings.TimeControl.Minutes
               && playerSettings.TimeControl.Increment == challengeToJoin.ChallengeSettings.TimeControl.Increment
               && playerRating >= challengeToJoin.ChallengeSettings.EloRatingPreference.Min
               && playerRating <= challengeToJoin.ChallengeSettings.EloRatingPreference.Max
               && challengeToJoin.PlayerRating >= playerSettings.EloRatingPreference.Min
               && challengeToJoin.PlayerRating <= playerSettings.EloRatingPreference.Max
               && (playerSettings.ColorPreference == ColorPreference.Any
                   || challengeToJoin.ChallengeSettings.ColorPreference == ColorPreference.Any 
                   || playerSettings.ColorPreference == ColorPreference.White 
                       && challengeToJoin.ChallengeSettings.ColorPreference == ColorPreference.Black
                   || playerSettings.ColorPreference == ColorPreference.Black
                       && challengeToJoin.ChallengeSettings.ColorPreference == ColorPreference.White);
    }
    
    private async Task AddAsync(Challenge challenge, CancellationToken cancellationToken = default)
    {
        var key = $"{ChallengeKeyPrefix}:{challenge.PlayerId}";

        if (await _database.KeyExistsAsync(key))
            throw new KeyAlreadyExistsException(key);

        var hashEntries = GetHashEntriesFromChallenge(challenge);
        await _database.HashSetAsync(key, hashEntries);
    }

    private async Task DeleteAsync(Challenge chat, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{ChallengeKeyPrefix}:{chat.PlayerId}");

    private IEnumerable<Challenge> GetAll()
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
