using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public class GetCompletedGameQueryHandler(
    ICompletedGameRepository completedGameRepository) : IRequestHandler<GetCompletedGameQuery, CompletedGameReadModel> 
{
    public async Task<CompletedGameReadModel> Handle(GetCompletedGameQuery request, CancellationToken cancellationToken)
    {
        var completedGame = await completedGameRepository.GetByIdAsync(request.GameId, cancellationToken)
                            ?? throw new NotFoundException(nameof(CompletedGameReadModel), request.GameId);
        
        return completedGame;
    }
}