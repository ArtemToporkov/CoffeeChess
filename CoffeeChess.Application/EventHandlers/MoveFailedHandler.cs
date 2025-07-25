using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Events;
using MediatR;

namespace CoffeeChess.Application.EventHandlers;

public class MoveFailedHandler(IGameEventNotifier notifier) : INotificationHandler<MoveFailed>
{
    public async Task Handle(MoveFailed notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyMoveFailed(notification.MoverId, notification.Reason);
    }
}