using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public record GetCompletedGamesCountQuery(string PlayerId) : IRequest<int>;