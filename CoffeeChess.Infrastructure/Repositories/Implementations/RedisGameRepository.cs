using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Infrastructure.Persistence.Models;
using CoffeeChess.Infrastructure.Serialization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class RedisGameRepository(
    IServiceProvider serviceProvider,
    IConnectionMultiplexer redis) : IGameRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private static readonly JsonSerializerOptions GameSerializationOptions = GetGameSerializationOptions();
    private const string GameKeyPrefix = "game";
    private const string MetadataKeySuffix = "metadata";
    private const string MovesHistoryKeySuffix = "moveshistory";
    private const string PositionsForThreeFoldKeySuffix = "positions";
    private const string GetGameScript = """
                                             local metadata = redis.call('HGETALL', KEYS[1])
                                             if #metadata == 0 then
                                                 return nil
                                             end
                                             local movesHistory = redis.call('LRANGE', KEYS[2], 0, -1)
                                             local positions = redis.call('HGETALL', KEYS[3])
                                             return {metadata, movesHistory, positions}
                                         """;

    public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var keys = new RedisKey[]
        {
            GetMetadataKey(id),
            GetMovesHistoryKey(id),
            GetPositionsForThreefoldKey(id)
        };

        var result = await _database.ScriptEvaluateAsync(GetGameScript, keys);
        if (result.IsNull) 
            return null;

        var resultArray = (RedisResult[])result!;
        var metadataValues = (RedisValue[])resultArray[0]!;
        var metadata = new HashEntry[metadataValues.Length / 2];
        for (var i = 0; i < metadataValues.Length; i += 2)
            metadata[i / 2] = new HashEntry(metadataValues[i], metadataValues[i + 1]);

        var movesHistory = (RedisValue[])resultArray[1]!;
        var positionsValues = (RedisValue[])resultArray[2]!;
        var positionsForThreefold = new HashEntry[positionsValues.Length / 2];
        for (var i = 0; i < positionsValues.Length; i += 2)
            positionsForThreefold[i / 2] = new HashEntry(positionsValues[i], positionsValues[i + 1]);

        var model = new GamePersistenceModel(metadata, positionsForThreefold, movesHistory);
        return model.ToGame(GameSerializationOptions);
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        var gamePersistenceModel = GamePersistenceModel.FromGame(game, serializerOptions: GameSerializationOptions);
        var metadata = gamePersistenceModel.StaticMetadata
            .Concat(gamePersistenceModel.MetadataThatCanUpdate)
            .ToArray();
        var transaction = _database.CreateTransaction();
        _ = transaction.HashSetAsync(
            GetMetadataKey(game.GameId), metadata);
        _ = transaction.HashSetAsync(
            GetPositionsForThreefoldKey(game.GameId), gamePersistenceModel.PositionsForThreefold);
        if (gamePersistenceModel.MovesHistory.Length > 0)
            _ = transaction.ListRightPushAsync(
                GetMovesHistoryKey(game.GameId), gamePersistenceModel.MovesHistory);
        await transaction.ExecuteAsync();
    }

    public async Task DeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        var transaction = _database.CreateTransaction();
        _ = transaction.KeyDeleteAsync(GetMetadataKey(game.GameId));
        _ = transaction.KeyDeleteAsync(GetMovesHistoryKey(game.GameId));
        _ = transaction.KeyDeleteAsync(GetPositionsForThreefoldKey(game.GameId));
        await transaction.ExecuteAsync();
    }

    public async Task SaveChangesAsync(Game game, CancellationToken cancellationToken = default)
    {
        var gamePersistenceModel = GamePersistenceModel.FromGame(game, serializerOptions: GameSerializationOptions);
        var positionsKey = GetPositionsForThreefoldKey(game.GameId);
        var movesHistoryKey = GetMovesHistoryKey(game.GameId);

        var transaction = _database.CreateTransaction();
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in game.DomainEvents)
        {
            if (@event is MoveMade moveMade)
            {
                _ = transaction.ListRightPushAsync(
                    movesHistoryKey, JsonSerializer.Serialize(moveMade.MoveInfo, GameSerializationOptions));
                // TODO: check if it's possible to not overwrite the whole dictionary
                _ = transaction.KeyDeleteAsync(positionsKey);
                _ = transaction.HashSetAsync(positionsKey, gamePersistenceModel.PositionsForThreefold);
            }

            await mediator.Publish(@event, cancellationToken);
        }

        _ = transaction.HashSetAsync(GetMetadataKey(game.GameId), gamePersistenceModel.MetadataThatCanUpdate);
        await transaction.ExecuteAsync();
        game.ClearDomainEvents();
    }

    public IEnumerable<Game> GetActiveGames()
    {
        // TODO: implement
        return [];
    }

    private static string GetMetadataKey(string id)
        => $"{GameKeyPrefix}:{id}:{MetadataKeySuffix}";

    private static string GetMovesHistoryKey(string id)
        => $"{GameKeyPrefix}:{id}:{MovesHistoryKeySuffix}";

    private static string GetPositionsForThreefoldKey(string id)
        => $"{GameKeyPrefix}:{id}:{PositionsForThreeFoldKeySuffix}";

    private static JsonSerializerOptions GetGameSerializationOptions()
        => new()
        {
            Converters =
            {
                new MoveInfoConverter(),
                new SanConverter(),
                new FenConverter()
            }
        };
}