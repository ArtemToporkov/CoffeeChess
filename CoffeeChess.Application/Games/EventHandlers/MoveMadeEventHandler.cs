using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Games.Services.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class MoveMadeEventHandler(
    IGameRepository gameRepository,
    IPgnBuilderService pgnBuilder,
    IGameEventNotifierService notifier) : INotificationHandler<MoveMade> 
{
    public async Task Handle(MoveMade notification, CancellationToken cancellationToken)
    {
        // TODO: use only the FEN or MoveInfo instead of the PGN for notifications,
        // so you can remove a list of MoveInfo from notification
        var sanMoves = notification.MovesHistory.Select(move => move.San).ToList();
        await notifier.NotifyMoveMade(notification.WhiteId, notification.BlackId, 
            pgnBuilder.GetPgnWithMovesOnly(sanMoves), notification.WhiteTimeLeft.TotalMilliseconds, 
            notification.BlackTimeLeft.TotalMilliseconds, cancellationToken);
    }
}