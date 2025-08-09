using CoffeeChess.Application.Games.ReadModels;
using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public record GetCompletedGamesPageQuery(string PlayerId, int PageNumber, int PageSize) 
    : IRequest<IReadOnlyList<CompletedGameReadModel>>;