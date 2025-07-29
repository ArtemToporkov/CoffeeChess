using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Events;
using CoffeeChess.Domain.Events.Game;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Domain.Services.Interfaces;
using MediatR;

namespace CoffeeChess.Application.EventHandlers;

public class GameEventsHandler(
    IRatingService ratingService,
    IPlayerRepository playerRepository, 
    IGameEventNotifierService notifier) : INotificationHandler<DrawOfferDeclined>,
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
        var (newWhiteRating, newBlackRating) = ratingService.CalculateNewRatings(
            notification.White.Rating, notification.Black.Rating, 
            notification.GameResult);

        await UpdateRatingAndSave(notification.White.Id, newWhiteRating);
        await UpdateRatingAndSave(notification.Black.Id, newBlackRating);
        
        await notifier.NotifyGameResultUpdated(notification.White, notification.Black,
        notification.GameResult, notification.WhiteReason, notification.BlackReason);
    }
    
    public async Task Handle(MoveFailed notification, CancellationToken cancellationToken)
        => await notifier.NotifyMoveFailed(notification.MoverId, notification.Reason);
    
    public async Task Handle(MoveMade notification, CancellationToken cancellationToken)
        => await notifier.NotifyMoveMade(notification.WhiteId, notification.BlackId, 
            notification.NewPgn, notification.WhiteTimeLeft.TotalMilliseconds, 
            notification.BlackTimeLeft.TotalMilliseconds);

    private async Task UpdateRatingAndSave(string playerId, int newRating)
    {
        var player = await playerRepository.GetAsync(playerId) ?? throw new InvalidOperationException(
            $"[{nameof(GameEventsHandler)}.{nameof(UpdateRatingAndSave)}]: player not found.");
        player.UpdateRating(newRating);
        await playerRepository.SaveChangesAsync(player);
    }
}