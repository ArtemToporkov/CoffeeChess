using CoffeeChess.Application.Interfaces;
using CoffeeChess.Application.Payloads;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Services;

public class SignalRGameEventNotifierService(
    IHubContext<GameHub, IGameClient> hubContext) : IGameEventNotifierService
{
    public async Task NotifyMoveMade(string whiteId,
        string blackId, string pgn, double whiteTimeLeft, double blackTimeLeft)
    {
        await hubContext.Clients.Users(whiteId, blackId).MakeMove(pgn, whiteTimeLeft, blackTimeLeft);
    }

    public async Task NotifyMoveFailed(string moverId, string reason)
    {
        await hubContext.Clients.User(moverId).MoveFailed(reason);
    }

    public async Task NotifyGameResultUpdated(Player white, Player black, GameResult gameResult,
        string whiteReason, string blackReason)
    {
        await hubContext.Clients.User(white.Id).UpdateGameResult(gameResult, whiteReason);
        await hubContext.Clients.User(black.Id).UpdateGameResult(gameResult, blackReason);
    }

    public async Task NotifyDrawOfferSent(string message, string senderId, string receiverId)
    {
        var offerPayload = new GameActionPayloadModel
        {
            GameActionType = GameActionType.ReceiveDrawOffer,
            Message = $"{message} offers a draw."
        };
        await hubContext.Clients.User(receiverId).PerformGameAction(offerPayload);
        var sendingPayload = new GameActionPayloadModel { GameActionType = GameActionType.SendDrawOffer };
        await hubContext.Clients.User(senderId).PerformGameAction(sendingPayload);
    }

    public async Task NotifyDrawOfferDeclined(string rejectingId, string senderId)
    {
        var senderPayload = new GameActionPayloadModel { GameActionType = GameActionType.GetDrawOfferDeclination };
        await hubContext.Clients.User(senderId).PerformGameAction(senderPayload);
        var rejectingPayload = new GameActionPayloadModel { GameActionType = GameActionType.DeclineDrawOffer };
        await hubContext.Clients.User(rejectingId).PerformGameAction(rejectingPayload);
    }
}