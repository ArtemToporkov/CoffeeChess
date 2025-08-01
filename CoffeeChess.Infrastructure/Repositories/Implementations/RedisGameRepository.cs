using System.Text.Json;
using CoffeeChess.Domain.Games.AggregatesRoots;
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

    public async Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var redisValue = await _database.StringGetAsync($"{GameKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Game>(redisValue!);
    }

    public async Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(game);
        await _database.StringSetAsync($"{GameKeyPrefix}:{game.GameId}", serializedGame, when: When.NotExists);
    }

    public async Task DeleteAsync(Game game, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{GameKeyPrefix}:{game.GameId}");

    public async Task SaveChangesAsync(Game game, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(game);
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
}