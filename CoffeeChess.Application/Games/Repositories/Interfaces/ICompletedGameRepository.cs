using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Shared.Abstractions;

namespace CoffeeChess.Application.Games.Repositories.Interfaces;

public interface ICompletedGameRepository
{
    public Task AddAsync(CompletedGameReadModel game);
    
    public Task<CompletedGameReadModel?> GetCompletedGameByIdAsync(string gameId, 
        CancellationToken cancellationToken = default);

    public Task<int> GetCompletedGamesCountForPlayerAsync(string playerId, 
        CancellationToken cancellationToken = default);
    
    public Task<PagedResult<CompletedGameReadModel>> GetCompletedGamesForPlayerAsync(string playerId,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}