using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class SqlPlayerRepository(ApplicationDbContext dbContext, IMediator mediator) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(string id) => await dbContext.Players
        .FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Player entity)
    {
        await dbContext.Players.AddAsync(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Player player)
    {
        dbContext.Players.Remove(player);
        await dbContext.SaveChangesAsync();
    }

    public IAsyncEnumerable<Player> GetAllAsync() => dbContext.Players.AsAsyncEnumerable();

    public async Task SaveChangesAsync(Player player)
    {
        await dbContext.SaveChangesAsync();
        
        foreach (var @event in player.DomainEvents)
            await mediator.Publish(@event);
        player.ClearDomainEvents();
    }
}