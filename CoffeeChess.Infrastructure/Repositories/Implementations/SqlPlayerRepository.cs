using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class SqlPlayerRepository(ApplicationDbContext dbContext, IMediator mediator) : IPlayerRepository
{
    public async Task<Player?> GetAsync(string id)
    {
        var player = await dbContext.Players.FirstOrDefaultAsync(p => p.Id == id);
        return player;
    }

    public async Task SaveChangesAsync(Player player)
    {
        await dbContext.SaveChangesAsync();
        
        foreach (var @event in player.DomainEvents)
            await mediator.Publish(@event);
        player.ClearDomainEvents();
    }
}