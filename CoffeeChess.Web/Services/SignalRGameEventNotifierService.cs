using CoffeeChess.Application.Games.Payloads;
using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Services;

public class SignalRGameEventNotifierService(IPlayerRepository playerRepository,
    IHubContext<GameHub, IGameClient> hubContext) : IGameEventNotifierService
{
    public async Task NotifyMoveMade(string whiteId,
        string blackId, string pgn, double whiteTimeLeft, double blackTimeLeft)
    {
        await hubContext.Clients.Users(whiteId, blackId).MoveMade(pgn, whiteTimeLeft, blackTimeLeft);
    }

    public async Task NotifyMoveFailed(string moverId, string reason)
    {
        await hubContext.Clients.User(moverId).MoveFailed(reason);
    }

    public async Task NotifyGameResultUpdated(Player white, Player black, GameResult gameResult,
        string whiteReason, string blackReason)
    {
        await hubContext.Clients.User(white.Id).GameResultUpdated(gameResult, whiteReason);
        await hubContext.Clients.User(black.Id).GameResultUpdated(gameResult, blackReason);
    }

    public async Task NotifyDrawOfferSent(string message, string senderId, string receiverId)
    {
        var offerPayload = new GameActionPayloadModel
        {
            GameActionType = GameActionType.ReceiveDrawOffer,
            Message = $"{message} offers a draw."
        };
        await hubContext.Clients.User(receiverId).GameActionPerformed(offerPayload);
        var sendingPayload = new GameActionPayloadModel { GameActionType = GameActionType.SendDrawOffer };
        await hubContext.Clients.User(senderId).GameActionPerformed(sendingPayload);
    }

    public async Task NotifyDrawOfferDeclined(string rejectingId, string senderId)
    {
        var senderPayload = new GameActionPayloadModel { GameActionType = GameActionType.GetDrawOfferDeclination };
        await hubContext.Clients.User(senderId).GameActionPerformed(senderPayload);
        var rejectingPayload = new GameActionPayloadModel { GameActionType = GameActionType.DeclineDrawOffer };
        await hubContext.Clients.User(rejectingId).GameActionPerformed(rejectingPayload);
    }

    public async Task NotifyGameStarted(string gameId, string whitePlayerId, string blackPlayerId,
        int totalMillisecondsForOnePlayerLeft)
    {
        var whitePlayer = await playerRepository.GetAsync(whitePlayerId);
        var whiteInfo = new PlayerInfoViewModel(whitePlayer!.Name, whitePlayer.Rating);
        var blackPlayer = await playerRepository.GetAsync(whitePlayerId);
        var blackInfo = new PlayerInfoViewModel(blackPlayer!.Name, whitePlayer.Rating);
        await hubContext.Clients.User(whitePlayerId).GameStarted(
            gameId, true, whiteInfo, blackInfo,
            totalMillisecondsForOnePlayerLeft);
        await hubContext.Clients.User(blackPlayerId).GameStarted(
            gameId, false, whiteInfo, blackInfo,
            totalMillisecondsForOnePlayerLeft);
    }
}