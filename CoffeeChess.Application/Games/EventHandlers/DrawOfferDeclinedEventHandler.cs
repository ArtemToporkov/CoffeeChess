using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Events;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class DrawOfferDeclinedEventHandler(
    IGameEventNotifierService notifier) : INotificationHandler<DrawOfferDeclined>
{
    public async Task Handle(DrawOfferDeclined notification, CancellationToken cancellationToken)
        => await notifier.NotifyDrawOfferDeclined(notification.RejectingId, notification.SenderId);
}