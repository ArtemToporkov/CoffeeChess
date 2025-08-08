using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Services.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class MoveMadeEventHandler(
    IPgnBuilderService pgnBuilder,
    IGameEventNotifierService notifier) : INotificationHandler<MoveMade> 
{
    public async Task Handle(MoveMade notification, CancellationToken cancellationToken)
    {
        var sanMoves = notification.MovesHistory.Select(m => m.San).ToList().AsReadOnly();
        await notifier.NotifyMoveMade(notification.WhiteId, notification.BlackId, 
            pgnBuilder.GetPgnWithMovesOnly(sanMoves), notification.WhiteTimeLeft.TotalMilliseconds, 
            notification.BlackTimeLeft.TotalMilliseconds, cancellationToken);
    }
}