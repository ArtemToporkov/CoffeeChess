using CoffeeChess.Application.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public class GetCompletedGamesCountQueryHandler(
    ICompletedGameRepository gameRepository) : IRequestHandler<GetCompletedGamesCountQuery, int>
{
    public async Task<int> Handle(GetCompletedGamesCountQuery request, CancellationToken cancellationToken)
        => await gameRepository.GetCompletedGamesCountForPlayerAsync(request.PlayerId, cancellationToken);
}