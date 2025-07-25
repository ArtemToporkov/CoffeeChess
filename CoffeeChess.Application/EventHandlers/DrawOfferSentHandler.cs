using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Events;
using MediatR;

namespace CoffeeChess.Application.EventHandlers;

public class DrawOfferSentHandler(IGameEventNotifier notifier) : INotificationHandler<DrawOfferSent>
{
    public async Task Handle(DrawOfferSent notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyDrawOfferSent(notification.SenderName, notification.SenderId, notification.ReceiverId);
    }
}