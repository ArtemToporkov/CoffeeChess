using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Entities;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
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

    public async Task<Game?> GetByIdAsync(string id)
    {
        var redisValue = await _database.StringGetAsync($"{GameKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        var gameState = JsonSerializer.Deserialize<GameState>(redisValue!);
        if (gameState is null)
            return null;
        
        return new Game(gameState);
    }

    public async Task AddAsync(Game game)
    {
        var gameState = game.GetGameState();
        var serializedGameState = JsonSerializer.Serialize(gameState);
        await _database.StringSetAsync($"{GameKeyPrefix}:{game.GameId}", serializedGameState, when: When.NotExists);
    }

    public async Task DeleteAsync(Game game)
        => await _database.KeyDeleteAsync($"{GameKeyPrefix}:{game.GameId}");

    public IAsyncEnumerable<Game> GetAllAsync()
    {
        throw new NotImplementedException();
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
    
    public IEnumerable<Game> GetActiveGames()
    {
        // TODO: implement
        return [];
    }
}