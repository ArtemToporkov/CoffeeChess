using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Notifications;
using MediatR;

namespace CoffeeChess.Web.BackgroundWorkers;

public class GameTimeoutBackgroundWorker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishTimeout(serviceProvider, stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }

    private static async Task PublishTimeout(IServiceProvider serviceProvider,
        CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var gameManager = scope.ServiceProvider.GetService<IGameManagerService>();
        var mediator = scope.ServiceProvider.GetService<IMediator>();
        if (gameManager is null || mediator is null)
            throw new InvalidOperationException(
                $"[{nameof(GameTimeoutBackgroundWorker)}.{nameof(ExecuteAsync)}]: one of requested services is null");
        foreach (var game in gameManager.GetActiveGames())
        {
            if (!game.UpdateTimeAndCheckTimeout(game.CurrentPlayerColor)) continue;
            game.Resign(game.CurrentPlayerColor);
            var (winner, loser) = game.GetWinnerAndLoser();
            if (winner is null || loser is null)
                throw new InvalidOperationException(
                    $"[{nameof(GameTimeoutBackgroundWorker)}.{nameof(PublishTimeout)}]: game is not ended.");
            await mediator.Publish(new GameEndedNotification
            {
                Winner = winner,
                Loser = loser,
                WinReason = $"{loser.Name}'s time is up.",
                LoseReason = "your time is up."
            }, stoppingToken);
        }
    }
}