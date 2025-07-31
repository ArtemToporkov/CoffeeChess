using CoffeeChess.Application.Services.Interfaces;
using CoffeeChess.Domain.Events.Player;
using MediatR;

namespace CoffeeChess.Application.EventHandlers.Player;

public class PlayerRatingChangedEventHandler(
    IPlayerEventNotifierService notifier) : INotificationHandler<PlayerRatingChanged>
{
    public async Task Handle(PlayerRatingChanged notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyPlayerRatingChanged(
            notification.PlayerId, notification.OldRating, notification.NewRating);
    }
}