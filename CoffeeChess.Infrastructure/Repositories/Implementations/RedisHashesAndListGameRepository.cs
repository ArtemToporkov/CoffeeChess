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

        // TODO: perform in a transaction

        var allMetadata = gamePersistenceModel.StaticMetadata
            .Concat(gamePersistenceModel.MetadataThatCanUpdate)
            .ToArray();
        await _database.HashSetAsync(
            GetMetadataKey(game.GameId), allMetadata);

        await _database.HashSetAsync(
            GetPositionsForThreefoldKey(game.GameId), gamePersistenceModel.PositionsForThreefold);
        await _database.ListRightPushAsync(
            GetMovesHistoryKey(game.GameId), gamePersistenceModel.MovesHistory);
    }

    public async Task DeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        // TODO: perform in a transaction
        await _database.KeyDeleteAsync(GetMetadataKey(game.GameId));
        await _database.KeyDeleteAsync(GetMovesHistoryKey(game.GameId));
        await _database.KeyDeleteAsync(GetPositionsForThreefoldKey(game.GameId));
    }

    public async Task SaveChangesAsync(Game game, CancellationToken cancellationToken = default)
    {
        var gamePersistenceModel = GamePersistenceModel.FromGame(game, serializerOptions: GameSerializationOptions);
        var positionsKey = GetPositionsForThreefoldKey(game.GameId);
        var movesHistoryKey = GetMovesHistoryKey(game.GameId);

        // TODO: perform in a transaction
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in game.DomainEvents)
        {
            if (@event is MoveMade moveMade)
            {
                await _database.ListRightPushAsync(
                    movesHistoryKey, JsonSerializer.Serialize(moveMade.MoveInfo, GameSerializationOptions));
                // TODO: check if it's possible to not overwrite the whole dictionary
                await _database.KeyDeleteAsync(positionsKey);
                await _database.HashSetAsync(positionsKey, gamePersistenceModel.PositionsForThreefold);
            }

            await mediator.Publish(@event, cancellationToken);
        }

        await _database.HashSetAsync(GetMetadataKey(game.GameId), gamePersistenceModel.MetadataThatCanUpdate);
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