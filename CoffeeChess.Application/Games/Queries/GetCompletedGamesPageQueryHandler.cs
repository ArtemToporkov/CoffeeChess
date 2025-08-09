using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public class GetCompletedGamesPageQueryHandler(ICompletedGameRepository gameRepository)
    : IRequestHandler<GetCompletedGamesPageQuery, IReadOnlyList<CompletedGameReadModel>>
{
    public async Task<IReadOnlyList<CompletedGameReadModel>> Handle(GetCompletedGamesPageQuery request, CancellationToken cancellationToken)
        => await gameRepository.GetCompletedGamesForPlayerAsync(
            request.PlayerId, request.PageNumber, request.PageSize, cancellationToken);
}