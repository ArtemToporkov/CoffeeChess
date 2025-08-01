using CoffeeChess.Application.Players.Services.Interfaces;
using CoffeeChess.Domain.Players.Events;
using MediatR;

namespace CoffeeChess.Application.Players.EventHandlers;

public class PlayerRatingChangedEventHandler(
    IPlayerEventNotifierService notifier) : INotificationHandler<PlayerRatingChanged>
{
    public async Task Handle(PlayerRatingChanged notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyPlayerRatingChanged(
            notification.PlayerId, notification.OldRating, notification.NewRating);
    }
}