using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public class CheckForActiveGamesCommandHandler(
    IGameRepository gameRepository) : IRequestHandler<CheckForActiveGamesCommand, string?>
{
    public async Task<string?> Handle(CheckForActiveGamesCommand request, CancellationToken cancellationToken)
    {
        var activeGame = await gameRepository.CheckPlayerForActiveGames(request.PlayerId);
        return activeGame?.GameId;
    }
}