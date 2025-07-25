using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Events;
using MediatR;

namespace CoffeeChess.Application.EventHandlers;

public class MoveMadeHandler(IGameEventNotifier notifier) : INotificationHandler<MoveMade>
{
    public async Task Handle(MoveMade notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyMoveMade(notification.WhiteId, notification.BlackId, 
            notification.NewPgn, notification.WhiteTimeLeft.TotalMilliseconds, 
            notification.BlackTimeLeft.TotalMilliseconds);
    }
}