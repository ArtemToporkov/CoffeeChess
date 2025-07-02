using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Notifications;
using MediatR;

namespace CoffeeChess.Web.Handlers;

public class GameCalculationHandler(IMediator mediator, IRatingService ratingService)
    : INotificationHandler<GameEndedNotification>, INotificationHandler<GameDrawnNotification>
{
    public async Task Handle(GameEndedNotification notification, CancellationToken cancellationToken)
    {
        var (messageForWhite, messageForBlack) = notification.Winner == PlayerColor.White
            ? (notification.WinReason, notification.LoseReason)
            : (notification.LoseReason, notification.WinReason);
        var gameResultCalculatedNotification = CalculateGameResult(notification.WhitePlayerInfo, notification.BlackPlayerInfo,
            notification.Winner == PlayerColor.White ? Result.WhiteWins : Result.BlackWins,
            messageForWhite, messageForBlack);
        await mediator.Publish(gameResultCalculatedNotification, cancellationToken);
    }

    public async Task Handle(GameDrawnNotification notification, CancellationToken cancellationToken)
    {
        var gameResultCalculatedNotification = CalculateGameResult(notification.WhitePlayerInfo, notification.BlackPlayerInfo,
            Result.Draw,
            notification.DrawReason, notification.DrawReason);
        await mediator.Publish(gameResultCalculatedNotification, cancellationToken);
    }

    private GameResultCalculatedNotification CalculateGameResult(PlayerInfoModel white, PlayerInfoModel black, 
        Result result, 
        string messageForWhite, string messageForBlack)
    {
        var (whitesNewRating, blacksNewRating) = ratingService.CalculateNewRatings(
            white.Rating, black.Rating, result);
        var whitePayload = new GameResultPayloadModel
        {
            Result = result switch
            {
                Result.WhiteWins => GameResultForPlayer.Won,
                Result.BlackWins => GameResultForPlayer.Lost,
                Result.Draw => GameResultForPlayer.Draw,
                _ => throw new ArgumentException($"[GameHub.SendGameResult]: unexpected argument for {nameof(result)}")
            },
            Message = messageForWhite,
            OldRating = white.Rating,
            NewRating = whitesNewRating
        };
        var blackPayload = new GameResultPayloadModel
        {
            Result = result switch
            {
                Result.WhiteWins => GameResultForPlayer.Lost,
                Result.BlackWins => GameResultForPlayer.Won,
                Result.Draw => GameResultForPlayer.Draw,
                _ => throw new ArgumentException($"[GameHub.SendGameResult]: unexpected argument for {nameof(result)}")
            },
            Message = messageForBlack,
            OldRating = black.Rating,
            NewRating = blacksNewRating
        };
        return new GameResultCalculatedNotification
        {
            WhitePlayerInfo = white,
            BlackPlayerInfo = black,
            GameResultPayloadForWhite = whitePayload,
            GameResultPayloadForBlack = blackPayload
        };
    }
}