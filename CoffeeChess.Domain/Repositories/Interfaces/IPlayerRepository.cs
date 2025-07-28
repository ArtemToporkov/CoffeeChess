using CoffeeChess.Domain.Entities;

namespace CoffeeChess.Domain.Repositories.Interfaces;

public interface IPlayerRepository
{
    public Task<Player?> GetAsync(string id);

    public Task SaveChangesAsync(Player player);
}