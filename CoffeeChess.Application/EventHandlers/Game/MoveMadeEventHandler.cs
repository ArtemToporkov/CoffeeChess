using CoffeeChess.Application.Services.Interfaces;
using CoffeeChess.Domain.Events.Game;
using MediatR;

namespace CoffeeChess.Application.EventHandlers.Game;

public class MoveMadeEventHandler(
    IGameEventNotifierService notifier) : INotificationHandler<MoveMade> 
{
    public async Task Handle(MoveMade notification, CancellationToken cancellationToken)
        => await notifier.NotifyMoveMade(notification.WhiteId, notification.BlackId,
            notification.NewPgn, notification.WhiteTimeLeft.TotalMilliseconds,
            notification.BlackTimeLeft.TotalMilliseconds);
}