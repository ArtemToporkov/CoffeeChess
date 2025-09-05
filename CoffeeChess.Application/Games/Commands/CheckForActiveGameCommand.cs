using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public record CheckForActiveGameCommand(string PlayerId) : IRequest<string?>;