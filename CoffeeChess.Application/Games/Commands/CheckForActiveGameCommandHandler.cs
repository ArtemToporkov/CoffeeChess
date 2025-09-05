using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public class CheckForActiveGameCommandHandler(
    IGameRepository gameRepository) : IRequestHandler<CheckForActiveGameCommand, string?>
{
    public async Task<string?> Handle(CheckForActiveGameCommand request, CancellationToken cancellationToken)
        => await gameRepository.CheckPlayerForActiveGameAsync(request.PlayerId);
}