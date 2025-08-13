using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class SqlCompletedGameRepository(ApplicationDbContext dbContext) : ICompletedGameRepository
{
    public async Task AddAsync(CompletedGameReadModel game, CancellationToken cancellationToken = default)
    {
        await dbContext.CompletedGames.AddAsync(game, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
        
    public async Task<CompletedGameReadModel?> GetByIdAsync(string gameId, 
        CancellationToken cancellationToken = default)
        => await dbContext.CompletedGames.FirstOrDefaultAsync(g => g.GameId == gameId,
            cancellationToken);

    public async Task<int> GetCompletedGamesCountForPlayerAsync(string playerId, 
        CancellationToken cancellationToken = default) 
        => await dbContext.CompletedGames
            .Where(g => g.WhitePlayerId == playerId || g.BlackPlayerId == playerId)
            .CountAsync(cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<CompletedGameReadModel>> GetCompletedGamesForPlayerAsync(
        string playerId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var playersGame = dbContext.CompletedGames
            .Where(g => g.WhitePlayerId == playerId || g.BlackPlayerId == playerId)
            .OrderByDescending(g => g.PlayedDate);
        var toSkip = (pageNumber - 1) * pageSize;
        var items = await playersGame
            .Skip(toSkip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return items.AsReadOnly();
    }
}