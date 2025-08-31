using System.Reflection;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Exceptions;
using CoffeeChess.Infrastructure.Persistence.Models;
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
    private static readonly string MatchmakingScript = LoadScriptOrThrow(
        "CoffeeChess.Infrastructure.LuaScripts.matchmaking.lua");
    private static byte[]? _scriptSha;

    public async Task QueueOrFindMatchingChallenge(
        string playerId, ChallengeSettings settings, CancellationToken cancellationToken = default)
    {
        var playerRating = (await playerRepository.GetByIdAsync(playerId, cancellationToken))?.Rating
            ?? throw new NotFoundException(nameof(Player), playerId);
        var challenge = new Challenge(playerId, playerRating, settings);
        var matchingChallenge = await TryFindFirstMatchingChallengeAndRemove(challenge);
        if (matchingChallenge is null)
            await AddAsync(challenge);
        else
        {
            challenge.Accept(matchingChallenge);
            foreach (var @event in challenge.DomainEvents)
                await mediator.Publish(@event, cancellationToken);
            challenge.ClearDomainEvents();
        }
    }

    private async Task<Challenge?> TryFindFirstMatchingChallengeAndRemove(Challenge challenge)
    {
        if (_scriptSha == null)
        {
            var loadedScript = await LuaScript.Prepare(MatchmakingScript)
                .LoadAsync(_database.Multiplexer
                    .GetServer(_database.Multiplexer
                        .GetEndPoints()
                        .First()));
            _scriptSha = loadedScript.Hash;
        }

        var poolKey = GetChallengePoolKey(challenge.ChallengeSettings.TimeControl);
        var challengePersistenceModel = ChallengePersistenceModel.FromChallenge(challenge);
        var result = await _database.ScriptEvaluateAsync(
            _scriptSha,
            keys: [ poolKey ],
            values:
            [
                challengePersistenceModel.PlayerId,
                challengePersistenceModel.PlayerRating,
                challengePersistenceModel.MinEloRatingPreference,
                challengePersistenceModel.MaxEloRatingPreference,
                challengePersistenceModel.ColorPreference
            ]);
        if (result.IsNull)
            return null;
        var resultArray = (RedisResult[]?)result;
        if (resultArray is null)
            return null;
        
        var matchingChallenge = ChallengePersistenceModel.FromRedisResult(resultArray);
        var transaction = _database.CreateTransaction();
        AddDeleteToTransaction(transaction, challenge);
        AddDeleteToTransaction(transaction, matchingChallenge);
        await transaction.ExecuteAsync();
        return matchingChallenge;
    }

    private async Task AddAsync(Challenge challenge)
    {
        var poolKey = GetChallengePoolKey(challenge.ChallengeSettings.TimeControl);
        var metadataKey = GetChallengeMetadataKey(challenge.PlayerId);
        
        if (await _database.KeyExistsAsync(metadataKey))
            throw new KeyAlreadyExistsException(metadataKey);
        
        var metadata = ChallengePersistenceModel.FromChallenge(challenge).ToHashEntries();

        var transaction = _database.CreateTransaction();
        _ = transaction.HashSetAsync(metadataKey, metadata);
        _ = transaction.SortedSetAddAsync(poolKey, challenge.PlayerId, challenge.PlayerRating);
        await transaction.ExecuteAsync();
    }

    private static string GetChallengePoolKey(TimeControl timeControl)
        => $"{ChallengeKeyPrefix}:{timeControl.Minutes}+{timeControl.Increment}";
    
    private static string GetChallengeMetadataKey(string playerId) 
        => $"{ChallengeKeyPrefix}:{playerId}";

    private static void AddDeleteToTransaction(
        ITransaction transaction, Challenge challenge)
    {
        var metadataKey = GetChallengeMetadataKey(challenge.PlayerId);
        var poolKey = GetChallengePoolKey(challenge.ChallengeSettings.TimeControl);

        _ = transaction.KeyDeleteAsync(metadataKey);
        _ = transaction.SortedSetRemoveAsync(poolKey, challenge.PlayerId);
    }

    private static string LoadScriptOrThrow(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new FileNotFoundException($"Resource \"{resourceName}\" not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
