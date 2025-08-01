using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Players.AggregatesRoots;

namespace CoffeeChess.Application.Services.Interfaces;

public interface IGameEventNotifierService
{
    public Task NotifyMoveMade(string whiteId, string blackId, string pgn, double whiteTimeLeft, double blackTimeLeft);

    public Task NotifyMoveFailed(string moverId, string reason);

    public Task NotifyGameResultUpdated(Player whitePlayer, Player blackPlayer, 
        GameResult gameResult, string whiteReason, string blackReason);

    public Task NotifyDrawOfferSent(string message, string senderId, string receiverId);

    public Task NotifyDrawOfferDeclined(string rejectingId, string senderId);

    public Task NotifyGameStarted(string gameId, string whitePlayerId, string blackPlayerId,
        int totalMillisecondsForOnePlayerLeft);
}