using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Repositories.Interfaces;

public interface IGameRepository : IBaseRepository<Game>
{
    public IAsyncEnumerable<Game> GetFinishedByTimeoutGamesAsync();

    public Task<string?> CheckPlayerForActiveGameAsync(string playerId);
}