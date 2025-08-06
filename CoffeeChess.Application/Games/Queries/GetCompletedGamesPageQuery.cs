using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Shared.Abstractions;
using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public record GetCompletedGamesPageQuery(string PlayerId, int PageNumber, int PageSize) 
    : IRequest<PagedResult<CompletedGameReadModel>>;