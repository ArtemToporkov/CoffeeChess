using CoffeeChess.Application.Interfaces;
using CoffeeChess.Application.Payloads;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Services;

public class SignalRGameEventNotifierService(
    IHubContext<GameHub, IGameClient> hubContext,
    IGameFinisherService gameFinisher) : IGameEventNotifier
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

    public async Task NotifyGameResultUpdated(PlayerInfo whiteInfo, PlayerInfo blackInfo, Result result, 
        string whiteReason, string blackReason)
    {
        switch (result)
        {
            case Result.WhiteWon:
                await gameFinisher.SendWinResultAndSave(whiteInfo, blackInfo, whiteReason, blackReason);
                break;
            case Result.BlackWon:
                await gameFinisher.SendWinResultAndSave(blackInfo, whiteInfo, blackReason, whiteReason);
                break;
            case Result.Draw:
                await gameFinisher.SendDrawResultAndSave(whiteInfo, blackInfo, whiteReason);
                break;
        }
    }

    public async Task NotifyDrawOfferSent(string senderName, string senderId, string receiverId)
    {
        var offerPayload = new GameActionPayloadModel
        {
            GameActionType = GameActionType.ReceiveDrawOffer,
            Message = $"{senderName} offers a draw."
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