using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class SqlCompletedGameRepository(ApplicationDbContext dbContext) : ICompletedGameRepository
{
    public async Task<CompletedGameReadModel?> GetCompletedGameByIdAsync(string gameId, 
        CancellationToken cancellationToken = default)
        => await dbContext.CompletedGames.FirstOrDefaultAsync(g => g.GameId == gameId,
            cancellationToken: cancellationToken);
}