using CoffeeChess.Application.Services.Interfaces;
using CoffeeChess.Domain.Events.Game;
using MediatR;

namespace CoffeeChess.Application.EventHandlers.Game;

public class DrawOfferDeclinedEventHandler(
    IGameEventNotifierService notifier) : INotificationHandler<DrawOfferDeclined>
{
    public async Task Handle(DrawOfferDeclined notification, CancellationToken cancellationToken)
        => await notifier.NotifyDrawOfferDeclined(notification.RejectingId, notification.SenderId);
}