using CoffeeChess.Application.Commands;
using CoffeeChess.Domain.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.CommandHandlers;

public class MakeMoveCommandHandler(IGameRepository gameRepository) : IRequestHandler<MakeMoveCommand>
{
    public async Task Handle(MakeMoveCommand request, CancellationToken cancellationToken)
    {
        if (!gameRepository.TryGetValue(request.GameId, out var game))
            throw new InvalidOperationException(
                $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game not found.");

        if (game.IsOver)
            throw new InvalidOperationException(
                $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game is over.");

        game.ApplyMove(request.PlayerId, request.From, request.To, request.Promotion);
        await gameRepository.SaveChangesAsync(game);
    }
}