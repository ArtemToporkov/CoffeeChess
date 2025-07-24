using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Aggregates;

namespace CoffeeChess.Domain.Repositories.Interfaces;

public interface IGameRepository : IBaseRepository<Game>
{
    public IEnumerable<Game> GetActiveGames();
}