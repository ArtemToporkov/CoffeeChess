using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using CoffeeChess.Application.Shared.Abstractions;
using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public class GetCompletedGamesPageQueryHandler(ICompletedGameRepository gameRepository)
    : IRequestHandler<GetCompletedGamesPageQuery, PagedResult<CompletedGameReadModel>>
{
    public async Task<PagedResult<CompletedGameReadModel>> Handle(GetCompletedGamesPageQuery request, CancellationToken cancellationToken)
        => await gameRepository.GetCompletedGamesForPlayerAsync(
            request.PlayerId, request.PageNumber, request.PageSize, cancellationToken);
}