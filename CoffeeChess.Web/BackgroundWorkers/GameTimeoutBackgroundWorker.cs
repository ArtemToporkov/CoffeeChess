using CoffeeChess.Service.Interfaces;

namespace CoffeeChess.Web.BackgroundWorkers;

public class GameTimeoutBackgroundWorker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await SendTimeoutResultAndSave();
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task SendTimeoutResultAndSave()
    {
        using var scope = serviceProvider.CreateScope();
        var gameManager = scope.ServiceProvider.GetRequiredService<IGameManagerService>();
        var gameFinisher = scope.ServiceProvider.GetRequiredService<IGameFinisherService>();
        
        foreach (var game in gameManager.GetActiveGames())
        {
            if (!game.UpdateTimeAndCheckTimeout(game.CurrentPlayerColor)) continue;
            game.Resign(game.CurrentPlayerColor);
            var (winner, loser) = game.GetWinnerAndLoser();
            if (winner is null || loser is null)
                throw new InvalidOperationException(
                    $"[{nameof(GameTimeoutBackgroundWorker)}.{nameof(SendTimeoutResultAndSave)}]: game is not ended.");
            
            await gameFinisher.SendWinResultAndSave(winner, loser,
                $"{loser.Name}'s time is up.", "your time is up.");
        }
    }
}