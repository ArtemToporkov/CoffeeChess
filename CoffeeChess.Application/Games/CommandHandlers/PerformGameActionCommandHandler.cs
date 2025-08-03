using CoffeeChess.Application.Games.Commands;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.CommandHandlers;

public class PerformGameActionCommandHandler(
    IGameRepository gameRepository) : IRequestHandler<PerformGameActionCommand>
{
    public async Task Handle(PerformGameActionCommand request, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
                   ?? throw new NotFoundException(nameof(Game), request.GameId);

        switch (request.GameActionType)
        {
            case GameActionType.SendDrawOffer:
                game.OfferADraw(request.PlayerId);
                await gameRepository.SaveChangesAsync(game, cancellationToken);
                break;
            case GameActionType.AcceptDrawOffer:
                game.AcceptDrawOffer(request.PlayerId);
                await gameRepository.SaveChangesAsync(game, cancellationToken);
                break;
            case GameActionType.DeclineDrawOffer:
                game.DeclineDrawOffer(request.PlayerId);
                await gameRepository.SaveChangesAsync(game, cancellationToken);
                break;
            case GameActionType.Resign:
                game.Resign(request.PlayerId);
                await gameRepository.SaveChangesAsync(game, cancellationToken);
                break;
            case GameActionType.ReceiveDrawOffer:
            case GameActionType.GetDrawOfferDeclination:
                throw new ArgumentException("You can only perform active game actions, not passive.");
            default:
                throw new ArgumentException("Unknown game action type.");
        }
    }
}