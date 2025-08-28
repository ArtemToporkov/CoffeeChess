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
        var game = GetGame(metadata, movesHistory, positionsForThreefold);
        return game;
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        var metadata = GetMetadata(game);
        var positionsForThreeFold = GetPositionsForThreefold(game);
        var movesHistory = GetMovesHistory(game);

        // TODO: perform in a transaction

        await _database.HashSetAsync(GetMetadataKey(game.GameId), metadata);
        await _database.HashSetAsync(GetPositionsForThreefoldKey(game.GameId), positionsForThreeFold);
        await _database.ListRightPushAsync(GetMovesHistoryKey(game.GameId), movesHistory);
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
        var metadataToUpdate = GetMetadataToUpdate(game);
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
                await _database.HashSetAsync(positionsKey, GetPositionsForThreefold(game));
            }

            await mediator.Publish(@event, cancellationToken);
        }

        await _database.HashSetAsync(GetMetadataKey(game.GameId), metadataToUpdate);
        game.ClearDomainEvents();
    }

    public IEnumerable<Game> GetActiveGames()
    {
        // TODO: implement
        return [];
    }

    private static Game GetGame(HashEntry[] metadata, RedisValue[] movesHistory, HashEntry[] positionsForThreefold)
    {
        var game = (Game)RuntimeHelpers.GetUninitializedObject(typeof(Game));
        var propertiesToSet = new[]
        {
            nameof(Game.GameId), nameof(Game.WhitePlayerId), nameof(Game.BlackPlayerId), nameof(Game.IsOver),
            nameof(Game.InitialTimeForOnePlayer), nameof(Game.CurrentFen)
        };
        var timespanPropertiesToSet = new[]
        {
            nameof(Game.InitialTimeForOnePlayer), nameof(Game.Increment), nameof(Game.WhiteTimeLeft),
            nameof(Game.BlackTimeLeft)
        };
        var dateTimePropertiesToSet = new[] { nameof(Game.LastTimeUpdate) };
        var hashEntriesDictionary = metadata.ToDictionary(entry => entry.Name.ToString(), entry => entry.Value);
        SetProperties(game, hashEntriesDictionary, propertiesToSet);
        SetProperties(game, hashEntriesDictionary, timespanPropertiesToSet, isTimeSpan: true);
        SetProperties(game, hashEntriesDictionary, dateTimePropertiesToSet, isDateTime: true);

        var playerWithDrawOffer = (int)hashEntriesDictionary["playerWithDrawOffer"];
        if (playerWithDrawOffer != -1 && !Enum.IsDefined(typeof(PlayerColor), playerWithDrawOffer))
            throw new InvalidEnumArgumentException(
                nameof(playerWithDrawOffer), playerWithDrawOffer, typeof(PlayerColor));
        typeof(Game).GetProperty(nameof(Game.PlayerWithDrawOffer))!
            .SetValue(game, playerWithDrawOffer == -1 ? null : (PlayerColor)playerWithDrawOffer);
        
        var currentPlayerColor = (int)hashEntriesDictionary["currentPlayerColor"];
        if (!Enum.IsDefined(typeof(PlayerColor), currentPlayerColor))
            throw new InvalidEnumArgumentException(
                nameof(currentPlayerColor), currentPlayerColor, typeof(PlayerColor));
        typeof(Game).GetProperty(nameof(Game.PlayerWithDrawOffer))!
            .SetValue(game, (PlayerColor)currentPlayerColor);
        
        var moves = movesHistory
            .Select(move => JsonSerializer.Deserialize<MoveInfo>(move!))
            .ToList();
        var movesHistoryField = GetFieldOrThrow("_movesHistory");
        movesHistoryField.SetValue(game, moves);
        var positionsForThreefoldField = GetFieldOrThrow("_positionsForThreefold");
        positionsForThreefoldField.SetValue(game,
            positionsForThreefold.ToDictionary(entry => entry.Name.ToString(), entry => (int)entry.Value));
        return game;
    }

    private static void SetProperties(
        Game game,
        Dictionary<string, RedisValue> hashEntriesDictionary,
        string[] propertiesNames,
        bool? isTimeSpan = null, bool? isDateTime = null)
    {
        var type = typeof(Game);

        foreach (var propertyName in propertiesNames)
        {
            var camelCasePropertyName = $"{propertyName[0].ToString().ToLower()}{propertyName[1..]}";
            var propertyValue = hashEntriesDictionary[camelCasePropertyName];

            var backingField = type.GetField($"<{propertyName}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);

            if (backingField == null)
                throw new Exception($"Cannot find backing field for property \"{propertyName}\".");

            if (isTimeSpan.HasValue && isTimeSpan.Value)
                backingField.SetValue(game, TimeSpan.FromMicroseconds((double)propertyValue));
            else if (isDateTime.HasValue && isDateTime.Value)
            {
                if (!DateTime.TryParseExact(propertyValue!, "O", CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind, out var parsedDateTime))
                    throw new Exception(
                        $"Can't parse a redis hash of the property \"{propertyName}\": {propertyValue}");
                backingField.SetValue(game, parsedDateTime);
            }
            else
                backingField.SetValue(game, propertyValue);
        }
    }



    private static HashEntry[] GetMetadata(Game game)
    {
        var toUpdate = GetMetadataToUpdate(game).ToList();
        toUpdate.AddRange([
            new("gameId", game.GameId),
            new("whitePlayerId", game.WhitePlayerId),
            new("blackPlayerId", game.BlackPlayerId),
            new("initialTimeForOnePlayer", game.InitialTimeForOnePlayer.TotalMicroseconds),
            new("increment", game.Increment.TotalMicroseconds),
        ]);
        return toUpdate.ToArray();
    }

    private static HashEntry[] GetMetadataToUpdate(Game game)
        =>
        [
            new("isOver", game.IsOver),
            new("lastTimeUpdate", game.LastTimeUpdate.ToString("O")),
            new("whiteTimeLeft", game.WhiteTimeLeft.TotalMicroseconds),
            new("blackTimeLeft", game.BlackTimeLeft.TotalMicroseconds),
            new("currentPlayerColor", (int)game.CurrentPlayerColor),
            new("currentFen", (string)game.CurrentFen),
            new("playerWithDrawOffer", game.PlayerWithDrawOffer.HasValue ? (int)game.PlayerWithDrawOffer.Value : -1)
        ];

    private static HashEntry[] GetPositionsForThreefold(Game game)
    {
        const string positionsFieldName = "_positionsForThreefoldCount";
        var positionsField = GetFieldOrThrow(positionsFieldName);
        var positions = GetFieldValueOrThrow<Dictionary<string, int>>(positionsField, game);
        return positions
            .Select(kvp => new HashEntry(kvp.Key, kvp.Value))
            .ToArray();
    }

    private static RedisValue[] GetMovesHistory(Game game)
    {
        const string movesHistoryFieldName = "_movesHistory";
        var movesHistoryField = GetFieldOrThrow(movesHistoryFieldName);
        var movesHistory = GetFieldValueOrThrow<List<MoveInfo>>(movesHistoryField, game);
        return movesHistory
            .Select(move => (RedisValue)JsonSerializer.Serialize(move, GameSerializationOptions))
            .ToArray();
    }

    private static FieldInfo GetFieldOrThrow(string fieldName)
        => typeof(Game).GetField(
               fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
           ?? throw new MissingFieldException(nameof(Game), fieldName);

    private static T GetFieldValueOrThrow<T>(FieldInfo fieldInfo, Game game)
        => (T?)fieldInfo.GetValue(game)
           ?? throw new InvalidCastException(
               $"Can't cast field \"{fieldInfo.Name}\" to type \"{typeof(T).FullName}\".");

    private static string GetMetadataKey(string id)
        => $"{GameKeyPrefix}:{id}:{MetadataKeySuffix}";

    private static string GetMovesHistoryKey(string id)
        => $"{GameKeyPrefix}:{id}:{MovesHistoryKeySuffix}";

    private static string GetPositionsForThreefoldKey(string id)
        => $"{GameKeyPrefix}:{id}:{PositionsForThreeFoldKeySuffix}";

    private static JsonSerializerOptions GetGameSerializationOptions()
        => new()
        {
            Converters = { new MoveInfoConverter() }
        };
}