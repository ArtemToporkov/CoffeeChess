using CoffeeChess.Application.Games.Commands;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.CommandHandlers;

public class MakeMoveCommandHandler(IGameRepository gameRepository) : IRequestHandler<MakeMoveCommand>
{
    public async Task Handle(MakeMoveCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken) 
                   ?? throw new InvalidOperationException(
                       $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game not found.");

        if (game.IsOver)
            throw new InvalidOperationException(
                $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game is over.");

        game.ApplyMove(request.PlayerId, request.From, request.To, request.Promotion);
        await gameRepository.SaveChangesAsync(game, cancellationToken);
    }
}