using CoffeeChess.Domain.Players.AggregatesRoots;

namespace CoffeeChess.Domain.Players.Repositories.Interfaces;

public interface IPlayerRepository
{
    public Task<Player?> GetAsync(string id);

    public Task SaveChangesAsync(Player player);
}