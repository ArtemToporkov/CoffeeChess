using System.Text.Json;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence.Models;
using CoffeeChess.Infrastructure.Serialization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class RedisHashesAndListGameRepository(
    IServiceProvider serviceProvider,
    IConnectionMultiplexer redis) : IGameRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string GameKeyPrefix = "game";
    private const string MetadataKeySuffix = "metadata";
    private const string MovesHistoryKeySuffix = "moveshistory";
    private const string PositionsForThreeFoldKeySuffix = "positions";
    private static readonly JsonSerializerOptions GameSerializationOptions = GetGameSerializationOptions();

    public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var metadata = await _database.HashGetAllAsync(GetMetadataKey(id));
        if (metadata.Length == 0)
            return null;
        var movesHistory = await _database.ListRangeAsync(GetMovesHistoryKey(id));
        var positionsForThreefold = await _database.HashGetAllAsync(GetPositionsForThreefoldKey(id));
        var gamePersistenceModel = new GamePersistenceModel(metadata, positionsForThreefold, movesHistory);
        return gamePersistenceModel.ToGame(serializerOptions: GameSerializationOptions);
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