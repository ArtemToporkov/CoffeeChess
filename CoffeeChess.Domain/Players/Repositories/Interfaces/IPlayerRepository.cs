using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Players.Repositories.Interfaces;

public interface IPlayerRepository : IBaseRepository<Player>;