using CoffeeChess.Application.Games.Payloads;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Web.Models.ViewModels;

namespace CoffeeChess.Web.Hubs;

public interface IGameClient
{
    public Task GameStarted(string gameId, bool isWhite,
        PlayerInfoViewModel whitePlayer, PlayerInfoViewModel blackPlayer,
        double totalMillisecondsForOnePlayerLeft, CancellationToken cancellationToken = default);

    public Task ChatMessageReceived(string username, string message, CancellationToken cancellationToken = default);

    public Task MoveMade(string pgn, double whiteMillisecondsLeft, double blackMillisecondsLeft,
        CancellationToken cancellationToken = default);

    public Task MoveFailed(string message, CancellationToken cancellationToken = default);

    public Task PerformingGameActionFailed(string message, CancellationToken cancellationToken = default);

    public Task GameActionPerformed(GameActionPayloadModel payload, CancellationToken cancellationToken = default);

    public Task GameResultUpdated(GameResult gameResult, string? message,
        CancellationToken cancellationToken = default);

    public Task PlayerRatingUpdated(int oldRating, int newRating, CancellationToken cancellationToken = default);
}