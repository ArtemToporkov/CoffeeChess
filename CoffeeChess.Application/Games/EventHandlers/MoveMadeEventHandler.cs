using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Services.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class MoveMadeEventHandler(
    IPgnBuilderService pgnBuilder,
    IGameEventNotifierService notifier) : INotificationHandler<MoveMade> 
{
    public async Task Handle(MoveMade notification, CancellationToken cancellationToken)
    {
        await notifier.NotifyMoveMade(notification.WhiteId, notification.BlackId,
        pgnBuilder.GetPgn(notification.SanMovesHistory), notification.WhiteTimeLeft.TotalMilliseconds,
        notification.BlackTimeLeft.TotalMilliseconds, cancellationToken);
        Console.WriteLine(pgnBuilder.GetPgn(notification.SanMovesHistory));
    }
}