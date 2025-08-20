using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;
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
    private static readonly JsonSerializerOptions GameSerializationOptions = GetGameSerializationOptions();

    public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var redisValue = await _database.StringGetAsync($"{GameKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Game>(redisValue!, GameSerializationOptions);
    }

    public async Task AddAsync(Game gameChallenge, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(gameChallenge, GameSerializationOptions);
        await _database.StringSetAsync($"{GameKeyPrefix}:{gameChallenge.GameId}", serializedGame, when: When.NotExists);
    }

    public async Task DeleteAsync(Game game, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{GameKeyPrefix}:{game.GameId}");

    public async Task SaveChangesAsync(Game game, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(game, GameSerializationOptions);
        await _database.StringSetAsync($"{GameKeyPrefix}:{game.GameId}", serializedGame);

        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in game.DomainEvents)
            await mediator.Publish(@event, cancellationToken);
        game.ClearDomainEvents();
    }

    public IEnumerable<Game> GetActiveGames()
    {
        // TODO: implement
        return [];
    }

    private static JsonSerializerOptions GetGameSerializationOptions()
    {
        var jsonTypeResolver = new DefaultJsonTypeInfoResolver();
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
                if (propInfo is null) continue;
                propInfo.Set = prop.SetValue;
            }

            var propsToHide = jsonTypeInfo.Properties
                .Where(p => p.Name is nameof(Game.DomainEvents) or nameof(Game.MovesHistory))
                .ToList();
            propsToHide.ForEach(p => jsonTypeInfo.Properties.Remove(p));
            jsonTypeInfo.CreateObject = () =>
            {
                var game = (Game)RuntimeHelpers.GetUninitializedObject(typeof(Game));
                var domainEventsField = typeof(AggregateRoot<IDomainEvent>).GetField(
                    "_domainEvents", BindingFlags.NonPublic | BindingFlags.Instance) 
                                        ?? throw new SerializationException("Can't find \"_domainEvents\" field.");
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
}