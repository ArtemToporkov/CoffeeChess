using CoffeeChess.Domain.Aggregates;

namespace CoffeeChess.Domain.Repositories.Interfaces;

public interface IPlayerRepository
{
    public Task<Player?> GetAsync(string id);

    public Task SaveChangesAsync(Player player);
}