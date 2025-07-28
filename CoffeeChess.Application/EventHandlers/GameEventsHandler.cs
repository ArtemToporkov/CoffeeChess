using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Events;
using MediatR;

namespace CoffeeChess.Application.EventHandlers;

public class GameEventsHandler(IGameEventNotifierService notifier) : INotificationHandler<DrawOfferDeclined>,
    INotificationHandler<DrawOfferSent>,
    INotificationHandler<GameResultUpdated>,
    INotificationHandler<MoveFailed>,
    INotificationHandler<MoveMade>
{
    public async Task Handle(DrawOfferDeclined notification, CancellationToken cancellationToken) 
        => await notifier.NotifyDrawOfferDeclined(notification.RejectingId, notification.SenderId);
    
    public async Task Handle(DrawOfferSent notification, CancellationToken cancellationToken)
        => await notifier.NotifyDrawOfferSent(notification.SenderName, notification.SenderId, notification.ReceiverId);
    
    public async Task Handle(GameResultUpdated notification, CancellationToken cancellationToken)
    {
        
        await notifier.NotifyGameResultUpdated(notification.WhiteInfo, notification.BlackInfo,
        notification.Result, notification.WhiteReason, notification.BlackReason);
    }
    public async Task Handle(MoveFailed notification, CancellationToken cancellationToken)
        => await notifier.NotifyMoveFailed(notification.MoverId, notification.Reason);
    
    public async Task Handle(MoveMade notification, CancellationToken cancellationToken)
        => await notifier.NotifyMoveMade(notification.WhiteId, notification.BlackId, 
            notification.NewPgn, notification.WhiteTimeLeft.TotalMilliseconds, 
            notification.BlackTimeLeft.TotalMilliseconds);
}