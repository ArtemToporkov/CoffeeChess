using CoffeeChess.Application.Games.ReadModels;

namespace CoffeeChess.Application.Games.Repositories.Interfaces;

public interface ICompletedGameRepository
{
    public Task AddAsync(CompletedGameReadModel game, CancellationToken cancellationToken = default);
    
    public Task<CompletedGameReadModel?> GetCompletedGameByIdAsync(string gameId, 
        CancellationToken cancellationToken = default);

    public Task<int> GetCompletedGamesCountForPlayerAsync(string playerId, 
        CancellationToken cancellationToken = default);
    
    public Task<IReadOnlyList<CompletedGameReadModel>> GetCompletedGamesForPlayerAsync(string playerId,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}