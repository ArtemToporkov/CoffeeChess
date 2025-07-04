using CoffeeChess.Core.Enums;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Notifications;
using MediatR;

namespace CoffeeChess.Web.BackgroundWorkers;

public class GameTimeoutBackgroundWorker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var gameManager = scope.ServiceProvider.GetService<IGameManagerService>();
        var mediator = scope.ServiceProvider.GetService<IMediator>();
        if (gameManager is null || mediator is null)
            throw new InvalidOperationException(
                $"[{nameof(GameTimeoutBackgroundWorker)}.{nameof(ExecuteAsync)}]: one of requested services is null");
        while (!stoppingToken.IsCancellationRequested)
        {
            await PublishTimeout(gameManager, mediator, stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }

    private static async Task PublishTimeout(IGameManagerService gameManager, IMediator mediator, 
        CancellationToken stoppingToken)
    {
        foreach (var game in gameManager.GetActiveGames())
        {
            if (DateTime.UtcNow < game.TimeExpiresAt) continue;
            
            game.LoseOnTimeOrThrow();
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