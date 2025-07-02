using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Notifications;
using MediatR;

namespace CoffeeChess.Web.Handlers;

public class CalculateGameResultHandler(IMediator mediator, IRatingService ratingService)
    : INotificationHandler<GameDrawnNotification>, INotificationHandler<GameEndedNotification>
{
    public async Task Handle(GameDrawnNotification notification, CancellationToken cancellationToken)
    {
        var (newFirstRating, newSecondRating) = ratingService.CalculateNewRatingsAfterDraw(
            notification.FirstPlayer.Rating, notification.SecondPlayer.Rating);
        await mediator.Publish(CalculateGameResult(
                notification.FirstPlayer, notification.SecondPlayer,
                newFirstRating, newSecondRating,
                GameResultForPlayer.Draw, GameResultForPlayer.Draw,
                notification.DrawReason, notification.DrawReason),
            cancellationToken);
    }
    
    public async Task Handle(GameEndedNotification notification, CancellationToken cancellationToken)
    {
        var (newWinnerRating, newLoserRating) = ratingService.CalculateNewRatingsAfterWin(
            notification.Winner.Rating, notification.Loser.Rating);
        await mediator.Publish(CalculateGameResult(
            notification.Winner, notification.Loser,
            newWinnerRating, newLoserRating,
            GameResultForPlayer.Won, GameResultForPlayer.Lost,
            notification.WinReason, notification.LoseReason), 
            cancellationToken);
    }

    private GameResultCalculatedNotification CalculateGameResult(
        PlayerInfoModel firstPlayer, PlayerInfoModel secondPlayer, 
        int newFirstRating, int newSecondRating,
        GameResultForPlayer resultForFirstPlayer, GameResultForPlayer resultForSecondPlayer, 
        string messageForFirstPlayer, string messageForSecondPlayer)
    {
        var firstPayLoad = new GameResultPayloadModel(resultForFirstPlayer, messageForFirstPlayer,
            firstPlayer.Rating, newFirstRating);
        var secondPayload = new GameResultPayloadModel(resultForSecondPlayer, messageForSecondPlayer,
            secondPlayer.Rating, newSecondRating);
        return new GameResultCalculatedNotification
        {
            FirstPlayer = firstPlayer,
            SecondPlayer = secondPlayer,
            GameResultPayloadForFirst = firstPayLoad,
            GameResultPayloadForSecond = secondPayload
        };
    }
}