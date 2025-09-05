using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;
using CoffeeChess.Infrastructure.Mapping.Helpers;
using CoffeeChess.Infrastructure.Serialization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Repositories.Implementations.Games;

public class RedisGameRepository(
    IServiceProvider serviceProvider,
    IConnectionMultiplexer redis) : IGameRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string GameKeyPrefix = "game";
    private const string ActiveGameForPlayerKeySuffix = "activegame";
    private const string GameTimoutAtKey = "game:finishes:by:timout:at";
    private static readonly JsonSerializerOptions GameSerializationOptions = GetGameSerializationOptions();

    public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var redisValue = await _database.StringGetAsync($"{GameKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Game>(redisValue!, GameSerializationOptions);
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(game, GameSerializationOptions);
        var transaction = _database.CreateTransaction();
        _ = transaction.StringSetAsync(GetGameKey(game.GameId), serializedGame, when: When.NotExists);
        _ = transaction.StringSetAsync(
            GetPlayerActiveGameKey(game.WhitePlayerId), game.GameId, when: When.NotExists);
        _ = transaction.StringSetAsync(
            GetPlayerActiveGameKey(game.BlackPlayerId), game.GameId, when: When.NotExists);
        _ = transaction.SortedSetAddAsync(
            GameTimoutAtKey, game.GameId, GetGameTimeoutUnixMilliseconds(game));
        await transaction.ExecuteAsync();
    }

    public async Task DeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        var transaction = _database.CreateTransaction();
        _ = transaction.KeyDeleteAsync(GetGameKey(game.GameId));
        _ = transaction.KeyDeleteAsync(GetPlayerActiveGameKey(game.WhitePlayerId));
        _ = transaction.KeyDeleteAsync(GetPlayerActiveGameKey(game.BlackPlayerId));
        _ = transaction.SortedSetRemoveAsync(GameTimoutAtKey, game.GameId);
        await transaction.ExecuteAsync();
    }

    public async Task SaveChangesAsync(Game game, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(game, GameSerializationOptions);
        var transaction = _database.CreateTransaction();
        _ = transaction.StringSetAsync($"{GameKeyPrefix}:{game.GameId}", serializedGame);
        _ = transaction.SortedSetAddAsync(
            GameTimoutAtKey, game.GameId, GetGameTimeoutUnixMilliseconds(game));
        await transaction.ExecuteAsync();
        
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in game.DomainEvents)
            await mediator.Publish(@event, cancellationToken);
        game.ClearDomainEvents();
    }

    public async Task<string?> CheckPlayerForActiveGameAsync(string playerId)
    {
        var value = await _database.StringGetAsync(GetPlayerActiveGameKey(playerId));
        if (value.IsNull)
            return null;
        return value;
    }

    public async IAsyncEnumerable<Game> GetFinishedByTimeoutGamesAsync()
    {
        var nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var redisResult = await _database.SortedSetRangeByScoreAsync(
            GameTimoutAtKey, 0, nowTimestamp);
        foreach (var gameId in redisResult.Select(x => x.ToString()))
        {
            var game = await GetByIdAsync(gameId);
            yield return game!;
        }
    }

    private static string GetGameKey(string gameId)
        => $"{GameKeyPrefix}:{gameId}";

    private static string GetPlayerActiveGameKey(string playerId)
        => $"{playerId}:{ActiveGameForPlayerKeySuffix}";

    private static JsonSerializerOptions GetGameSerializationOptions()
    {
        var jsonTypeResolver = new DefaultJsonTypeInfoResolver();
        const string domainEventsFieldName = "_domainEvents";
        jsonTypeResolver.Modifiers.Add(jsonTypeInfo =>
        {
            if (jsonTypeInfo.Type != typeof(Game)) return;
            foreach (var field in typeof(Game).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(f => !f.Name.StartsWith('<')))
            {
                var fieldInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
                fieldInfo.Get = field.GetValue;
                fieldInfo.Set = field.SetValue;
                jsonTypeInfo.Properties.Add(fieldInfo);
            }

            foreach (var prop in typeof(Game).GetProperties(
                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                var propInfo = jsonTypeInfo.Properties.FirstOrDefault(p => p.Name == prop.Name);
                if (propInfo is null) 
                    continue;
                propInfo.Set = (obj, value) =>
                {
                    if (obj is Game game)
                        ReflectionMemberAccessHelper.SetPropertyValueOrThrow(game, prop.Name, value);
                    else
                        throw new InvalidCastException($"Can't cast object \"{obj}\" to {nameof(Game)}.");
                };
            }

            var propsToHide = jsonTypeInfo.Properties
                .Where(p => p.Name is nameof(Game.DomainEvents) or nameof(Game.MovesHistory))
                .ToList();
            propsToHide.ForEach(p => jsonTypeInfo.Properties.Remove(p));
            jsonTypeInfo.CreateObject = () =>
            {
                var game = (Game)RuntimeHelpers.GetUninitializedObject(typeof(Game));
                var domainEventsField = typeof(AggregateRoot<IDomainEvent>).GetField(
                                            domainEventsFieldName, 
                                            BindingFlags.NonPublic | BindingFlags.Instance) 
                                        ?? throw new SerializationException(
                                            $"Can't find \"{domainEventsFieldName}\" field.");
                domainEventsField.SetValue(game, new List<IDomainEvent>());
                return game;
            };
        });
        return new()
        {
            TypeInfoResolver = jsonTypeResolver,
            Converters = { new FenConverter(), new SanConverter() }
        };
    }

    private static long GetGameTimeoutUnixMilliseconds(Game game)
    {
        var timeoutsAfter = game.CurrentPlayerColor == PlayerColor.White 
            ? game.WhiteTimeLeft 
            : game.BlackTimeLeft;
        var timeoutsAt = new DateTimeOffset(game.LastTimeUpdate + timeoutsAfter).ToUnixTimeMilliseconds();
        return timeoutsAt;
    }
}