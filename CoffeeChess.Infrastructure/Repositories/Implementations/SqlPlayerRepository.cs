using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class SqlPlayerRepository(ApplicationDbContext dbContext) : IPlayerRepository
{
    public async Task<Player?> GetAsync(string id)
    {
        var player = await dbContext.Players.FirstOrDefaultAsync(p => p.Id == id);
        return player;
    }

    public async Task SaveChangesAsync(Player player)
    {
        await dbContext.SaveChangesAsync();
    }
}