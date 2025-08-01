using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class MoveFailedEventHandler(
    IGameEventNotifierService notifier) : INotificationHandler<MoveFailed>
{
    public async Task Handle(MoveFailed notification, CancellationToken cancellationToken)
        => await notifier.NotifyMoveFailed(notification.MoverId, GetMessageByMoveFailedReason(notification.Reason));
    
    private static string GetMessageByMoveFailedReason(MoveFailedReason reason) => reason switch
    {
        MoveFailedReason.InvalidMove => "Invalid move.",
        MoveFailedReason.TimeRanOut => "Your time is run up.",
        MoveFailedReason.NotYourTurn => "It's not your turn.",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };
}