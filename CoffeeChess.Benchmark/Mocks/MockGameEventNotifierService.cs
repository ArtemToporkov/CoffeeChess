using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Players.AggregatesRoots;

namespace CoffeeChess.Benchmark.Mocks;

public class MockGameEventNotifierService : IGameEventNotifierService
{
    public Task NotifyMoveMade(string whiteId, string blackId, string pgn, double whiteTimeLeft, double blackTimeLeft,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyMoveFailed(string moverId, string reason, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task NotifyGameEnded(Player whitePlayer, Player blackPlayer, GameResult gameResult, string whiteReason,
        string blackReason, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyDrawOfferSent(string message, string senderId, string receiverId,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyDrawOfferDeclined(string rejectingId, string senderId,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyGameStarted(string gameId, string whitePlayerId, string blackPlayerId,
        int totalMillisecondsForOnePlayerLeft, CancellationToken cancellationToken = default) => Task.CompletedTask;
}