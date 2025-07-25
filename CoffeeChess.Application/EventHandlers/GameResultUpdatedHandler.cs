using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Events;
using MediatR;

namespace CoffeeChess.Application.EventHandlers;

public class GameResultUpdatedHandler(IGameEventNotifier notifier) : INotificationHandler<GameResultUpdated>
{
    public async Task Handle(GameResultUpdated notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyGameResultUpdated(notification.WhiteInfo, notification.BlackInfo, 
            notification.Result, notification.WhiteReason, notification.BlackReason);
    }
}