using CoffeeChess.Application.Games.ReadModels;

namespace CoffeeChess.Application.Games.Repositories.Interfaces;

public interface ICompletedGameRepository
{
    public Task<CompletedGameReadModel?> GetCompletedGameByIdAsync(string gameId);
}