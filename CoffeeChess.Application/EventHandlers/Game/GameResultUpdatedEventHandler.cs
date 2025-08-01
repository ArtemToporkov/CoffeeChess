using CoffeeChess.Application.Services.Interfaces;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Services.Interfaces;
using MediatR;

namespace CoffeeChess.Application.EventHandlers.Game;

public class GameResultUpdatedEventHandler(
    IRatingService ratingService,
    IPlayerRepository playerRepository,
    IGameEventNotifierService notifier) : INotificationHandler<GameResultUpdated>
{
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
    
    private async Task UpdateRatingAndSave(string playerId, int newRating)
    {
        var player = await playerRepository.GetAsync(playerId) ?? throw new InvalidOperationException(
            $"[{nameof(GameResultUpdatedEventHandler)}.{nameof(UpdateRatingAndSave)}]: player not found.");
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
}