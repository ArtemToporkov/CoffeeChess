using CoffeeChess.Domain.Repositories.Interfaces;

namespace CoffeeChess.Web.HostedServices;

public class GameTimeoutCheckerService(IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
            foreach (var game in gameRepository.GetActiveGames())
            {
                game.CheckTimeout();
                await gameRepository.SaveChangesAsync(game);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}