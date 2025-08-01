using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class SqlPlayerRepository(ApplicationDbContext dbContext, IMediator mediator) : IPlayerRepository
{
    public async Task<Player?> GetByIdAsync(string id, CancellationToken cancellationToken = default) 
        => await dbContext.Players.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Player entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Players.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Player player, CancellationToken cancellationToken = default)
    {
        dbContext.Players.Remove(player);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(Player player, CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        
        foreach (var @event in player.DomainEvents)
            await mediator.Publish(@event, cancellationToken);
        player.ClearDomainEvents();
    }
}