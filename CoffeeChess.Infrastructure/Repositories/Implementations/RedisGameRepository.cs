using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Repositories.Interfaces;
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
    
    
    public bool TryGetValue(string id, [NotNullWhen(true)] out Game? game)
    {
        var redisValue = _database.StringGet($"{GameKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
        {
            game = null;
            return false;
        }

        var gameState = JsonSerializer.Deserialize<GameState>(redisValue!);

        if (gameState is null)
        {
            game = null;
            return false;
        }
        
        game = new Game(gameState);
        return true;
    }

    public bool TryAdd(string id, Game game)
    {
        var gameState = game.GetGameState();
        var serializedGameState = JsonSerializer.Serialize(gameState);
        return _database.StringSet($"{GameKeyPrefix}:{id}", serializedGameState, when: When.NotExists);
    }

    public bool TryRemove(string id, [NotNullWhen(true)] out Game? removedGame)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(string, Game)> GetAll()
    {
        return [];
    }

    public void SaveChanges(Game game)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Game> GetActiveGames()
    {
        // TODO: implement
        return [];
    }

    public async Task SaveChangesAsync(Game game)
    {
        var state = game.GetGameState();
        var serializedState = JsonSerializer.Serialize(state);
        await _database.StringSetAsync($"{GameKeyPrefix}:{game.GameId}", serializedState);
        
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in game.DomainEvents)
            await mediator.Publish(@event);
        game.ClearDomainEvents();
    }
}