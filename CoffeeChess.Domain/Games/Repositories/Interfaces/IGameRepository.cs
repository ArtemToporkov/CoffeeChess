using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Repositories.Interfaces;

public interface IGameRepository : IBaseRepository<Game>
{
    public IEnumerable<Game> GetActiveGames();

    public Task<Game?> CheckForActiveGames(string playerId);
}