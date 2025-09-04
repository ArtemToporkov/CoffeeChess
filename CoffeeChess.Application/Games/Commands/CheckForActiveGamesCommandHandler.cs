using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public class CheckForActiveGamesCommandHandler(
    IGameRepository gameRepository) : IRequestHandler<CheckForActiveGamesCommand, string?>
{
    public async Task<string?> Handle(CheckForActiveGamesCommand request, CancellationToken cancellationToken)
        => await gameRepository.CheckPlayerForActiveGames(request.PlayerId);
}