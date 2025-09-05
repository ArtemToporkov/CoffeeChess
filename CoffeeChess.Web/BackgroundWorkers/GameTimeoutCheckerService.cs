using CoffeeChess.Domain.Games.Repositories.Interfaces;

namespace CoffeeChess.Web.BackgroundWorkers;

public class GameTimeoutCheckerService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
            await foreach (var game in gameRepository.GetFinishedByTimeoutGamesAsync().WithCancellation(cancellationToken))
            {
                game.CheckTimeout();
                await gameRepository.SaveChangesAsync(game, cancellationToken);
            }
            await Task.Delay(1000, cancellationToken);
        }
    }
}