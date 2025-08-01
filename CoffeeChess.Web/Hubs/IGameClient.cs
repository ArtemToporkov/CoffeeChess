using CoffeeChess.Application.Payloads;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Web.Models.ViewModels;

namespace CoffeeChess.Web.Hubs;

public interface IGameClient
{
    public Task GameStarted(string gameId, bool isWhite, 
        PlayerInfoViewModel whitePlayer, PlayerInfoViewModel blackPlayer, double totalMillisecondsForOnePlayerLeft);

    public Task ChatMessageReceived(string username, string message);

    public Task CriticalErrorOccured(string message);

    public Task MoveMade(string pgn, double whiteMillisecondsLeft, double blackMillisecondsLeft);

    public Task MoveFailed(string message);

    public Task PerformingGameActionFailed(string message);

    public Task GameActionPerformed(GameActionPayloadModel payload);
    
    public Task GameResultUpdated(GameResult gameResult, string? message);

    public Task PlayerRatingUpdated(int oldRating, int newRating);
}