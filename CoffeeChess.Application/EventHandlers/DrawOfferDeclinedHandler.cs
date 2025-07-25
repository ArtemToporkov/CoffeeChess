using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Events;
using MediatR;

namespace CoffeeChess.Application.EventHandlers;

public class DrawOfferDeclinedHandler(IGameEventNotifier notifier) : INotificationHandler<DrawOfferDeclined>
{
    public async Task Handle(DrawOfferDeclined notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyDrawOfferDeclined(notification.RejectingId, notification.SenderId);
    }
}