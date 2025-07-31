using CoffeeChess.Application.Commands;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.CommandHandlers;

public class PerformGameActionCommandHandler(
    IGameRepository gameRepository) : IRequestHandler<PerformGameActionCommand>
{
    public async Task Handle(PerformGameActionCommand request, CancellationToken cancellationToken)
    {
        if (!gameRepository.TryGetValue(request.GameId, out var game))
            throw new InvalidOperationException(
                $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game not found.");

        if (game.IsOver)
            throw new InvalidOperationException(
                $"[{nameof(MakeMoveCommandHandler)}.{nameof(Handle)}]: game is over.");

        switch (request.GameActionType)
        {
            case GameActionType.SendDrawOffer:
                game.OfferADraw(request.PlayerId);
                await gameRepository.SaveChangesAsync(game);
                break;
            case GameActionType.AcceptDrawOffer:
                game.AcceptDrawOffer(request.PlayerId);
                await gameRepository.SaveChangesAsync(game);
                break;
            case GameActionType.DeclineDrawOffer:
                game.DeclineDrawOffer(request.PlayerId);
                await gameRepository.SaveChangesAsync(game);
                break;
            case GameActionType.Resign:
                game.Resign(request.PlayerId);
                await gameRepository.SaveChangesAsync(game);
                break;
        }
    }
}