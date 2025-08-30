using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class InMemoryGameRepository : IGameRepository
{
    private static readonly Dictionary<string, Game> Database = new();
    
    public Task<Game?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => Task.FromResult(Database.GetValueOrDefault(id));

    public Task AddAsync(Game game, CancellationToken cancellationToken = default)
    {
        Database.TryAdd(game.GameId, game);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Game game, CancellationToken cancellationToken = default)
    {
        Database.Remove(game.GameId);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(Game game, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public IEnumerable<Game> GetActiveGames()
    {
        return [];
    }
}