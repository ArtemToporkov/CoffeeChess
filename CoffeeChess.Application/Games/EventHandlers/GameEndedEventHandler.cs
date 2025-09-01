using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Services.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class GameEndedEventHandler(
    IRatingService ratingService,
    IPlayerRepository playerRepository,
    IGameEventNotifierService notifier) : INotificationHandler<GameEnded>
{
    public async Task Handle(GameEnded notification, CancellationToken cancellationToken)
    {
        var white = await playerRepository.GetByIdAsync(notification.WhiteId, cancellationToken) 
                    ?? throw new NotFoundException(nameof(Player), notification.WhiteId);
        
        var black = await playerRepository.GetByIdAsync(notification.BlackId, cancellationToken) 
                    ?? throw new NotFoundException(nameof(Player), notification.BlackId);

        var (whiteRating, blackRating) = (white.Rating, black.Rating);
        var (newWhiteRating, newBlackRating) = ratingService.CalculateNewRatings(
            white.Rating, black.Rating,
            notification.GameResult);

        var (whiteReason, blackReason) = GetMessageByGameResultReason(notification.GameResult,
            notification.GameResultReason, white.Name, black.Name);
        // TODO: send a result and a rating changes info
        await notifier.NotifyGameEnded(white, black, notification.GameResult,
            whiteReason, blackReason, cancellationToken);
    }

    private static (string WhiteReason, string BlackReason) GetMessageByGameResultReason(GameResult result,
        GameResultReason reason, string whiteName, string blackName) 
    {
        return reason switch
        {
            GameResultReason.OpponentResigned when result is GameResult.BlackWon 
                => ("you resigned.", $"{whiteName} resigned."),
            GameResultReason.OpponentResigned when result is GameResult.WhiteWon 
                => ($"{blackName} resigned.", "you resigned."),
            GameResultReason.OpponentTimeRanOut when result is GameResult.BlackWon
                => ("your time is run up.", $"{whiteName}'s time is run up."),
            GameResultReason.OpponentTimeRanOut when result is GameResult.WhiteWon
                => ($"{blackName}'s time is run up.", "your time is run up."),
            GameResultReason.Checkmate => ("checkmate.", "checkmate."),
            GameResultReason.Agreement => ("by agreement.", "by agreement."),
            GameResultReason.Stalemate => ("stalemate.", "stalemate."),
            GameResultReason.Threefold => ("by threefold.", "by threefold."),
            GameResultReason.FiftyMovesRule => ("by 50-moves rule.", "by 50-moves rule."),
            _ => throw new ArgumentException(
                $"Invalid game result and its reason combination: {result.ToString()} + {reason.ToString()}")
        };
    }
}