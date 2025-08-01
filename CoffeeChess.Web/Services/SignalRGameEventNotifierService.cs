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
    public async Task NotifyMoveMade(string whiteId, string blackId, string pgn, 
        double whiteTimeLeft, double blackTimeLeft, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.Users(whiteId, blackId).MoveMade(
            pgn, whiteTimeLeft, blackTimeLeft, cancellationToken);
    }

    public async Task NotifyMoveFailed(string moverId, string reason, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.User(moverId).MoveFailed(reason, cancellationToken);
    }

    public async Task NotifyGameResultUpdated(Player white, Player black, GameResult gameResult,
        string whiteReason, string blackReason, CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.User(white.Id).GameResultUpdated(gameResult, whiteReason, cancellationToken);
        await hubContext.Clients.User(black.Id).GameResultUpdated(gameResult, blackReason, cancellationToken);
    }

    public async Task NotifyDrawOfferSent(string message, string senderId, string receiverId, 
        CancellationToken cancellationToken = default)
    {
        var offerPayload = new GameActionPayloadModel
        {
            GameActionType = GameActionType.ReceiveDrawOffer,
            Message = $"{message} offers a draw."
        };
        await hubContext.Clients.User(receiverId).GameActionPerformed(offerPayload, cancellationToken);
        var sendingPayload = new GameActionPayloadModel { GameActionType = GameActionType.SendDrawOffer };
        await hubContext.Clients.User(senderId).GameActionPerformed(sendingPayload, cancellationToken);
    }

    public async Task NotifyDrawOfferDeclined(string rejectingId, string senderId, 
        CancellationToken cancellationToken = default)
    {
        var senderPayload = new GameActionPayloadModel { GameActionType = GameActionType.GetDrawOfferDeclination };
        await hubContext.Clients.User(senderId).GameActionPerformed(senderPayload, cancellationToken);
        var rejectingPayload = new GameActionPayloadModel { GameActionType = GameActionType.DeclineDrawOffer };
        await hubContext.Clients.User(rejectingId).GameActionPerformed(rejectingPayload, cancellationToken);
    }

    public async Task NotifyGameStarted(string gameId, string whitePlayerId, string blackPlayerId,
        int totalMillisecondsForOnePlayerLeft, CancellationToken cancellationToken = default)
    {
        var whitePlayer = await playerRepository.GetByIdAsync(whitePlayerId, cancellationToken) ?? throw new InvalidOperationException(
            $"[{nameof(SignalRGameEventNotifierService)}.{nameof(NotifyGameStarted)}]: white player not found.");
        var whiteInfo = new PlayerInfoViewModel(whitePlayer.Name, whitePlayer.Rating);
        
        var blackPlayer = await playerRepository.GetByIdAsync(blackPlayerId, cancellationToken) ?? throw new InvalidOperationException(
            $"[{nameof(SignalRGameEventNotifierService)}.{nameof(NotifyGameStarted)}]: black player not found.");
        var blackInfo = new PlayerInfoViewModel(blackPlayer.Name, blackPlayer.Rating);
        
        await hubContext.Clients.User(whitePlayerId).GameStarted(
            gameId, true, whiteInfo, blackInfo,
            totalMillisecondsForOnePlayerLeft, cancellationToken);
        await hubContext.Clients.User(blackPlayerId).GameStarted(
            gameId, false, whiteInfo, blackInfo,
            totalMillisecondsForOnePlayerLeft, cancellationToken);
    }
}