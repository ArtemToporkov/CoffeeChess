using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Enums;
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
    {
        var sender = await playerRepository.GetAsync(notification.SenderId);
        var receiver = await playerRepository.GetAsync(notification.ReceiverId);
        var message = $"{sender!.Name} offers a draw";
        await notifier.NotifyDrawOfferSent(message, sender.Id, receiver!.Id);
    }

    public async Task Handle(GameResultUpdated notification, CancellationToken cancellationToken)
    {
        var white = await playerRepository.GetAsync(notification.WhiteId);
        var black = await playerRepository.GetAsync(notification.BlackId);
        var (newWhiteRating, newBlackRating) = ratingService.CalculateNewRatings(
            white!.Rating, black!.Rating,
            notification.GameResult);

        await UpdateRatingAndSave(white.Id, newWhiteRating);
        await UpdateRatingAndSave(black.Id, newBlackRating);

        var (whiteReason, blackReason) = GetMessageByGameResultReason(
            notification.GameResultReason, white.Name, black.Name);
        await notifier.NotifyGameResultUpdated(white, black,
            notification.GameResult, whiteReason, blackReason);
    }

    public async Task Handle(MoveFailed notification, CancellationToken cancellationToken)
        => await notifier.NotifyMoveFailed(notification.MoverId, GetMessageByMoveFailedReason(notification.Reason));

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

    private (string WhiteReason, string BlackReason) GetMessageByGameResultReason(
        GameResultReason reason, string whiteName, string blackName)
        => reason switch
        {
            GameResultReason.WhiteResigns => ("you resigned.", $"{whiteName} resigned."),
            GameResultReason.BlackResigns => ($"{blackName} resigned.", "you resigned."),
            GameResultReason.WhiteTimeRanOut => ("your time is run up.", $"{whiteName}'s time is run up."),
            GameResultReason.BlackTimeRanOut => ($"{blackName}'s time is run up.", "your time is run up."),
            GameResultReason.WhiteCheckmates or 
                GameResultReason.BlackCheckmates => ("checkmate.", "checkmate."),
            GameResultReason.Agreement => ("by agreement.", "by agreement."),
            GameResultReason.Stalemate => ("stalemate.", "stalemate."),
            GameResultReason.Threefold => ("by threefold.", "by threefold."),
            GameResultReason.FiftyMovesRule => ("by 50-moves rule.", "by 50-moves rule."),
            _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
        };

    private string GetMessageByMoveFailedReason(MoveFailedReason reason) => reason switch
    {
        MoveFailedReason.InvalidMove => "Invalid move.",
        MoveFailedReason.TimeRanOut => "Your time is run up.",
        MoveFailedReason.NotYourTurn => "It's not your turn.",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };
}