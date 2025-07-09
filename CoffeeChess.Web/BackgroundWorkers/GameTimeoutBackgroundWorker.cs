using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

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
        var gameManager = scope.ServiceProvider.GetService<IGameManagerService>();
        var ratingService = scope.ServiceProvider.GetService<IRatingService>();
        var hubContext = scope.ServiceProvider.GetService<IHubContext<GameHub>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();
        if (gameManager is null || ratingService is null || hubContext is null)
            throw new InvalidOperationException(
                $"[{nameof(GameTimeoutBackgroundWorker)}.{nameof(ExecuteAsync)}]: one of requested services is null");
        
        foreach (var game in gameManager.GetActiveGames())
        {
            if (!game.UpdateTimeAndCheckTimeout(game.CurrentPlayerColor)) continue;
            game.Resign(game.CurrentPlayerColor);
            var (winner, loser) = game.GetWinnerAndLoser();
            if (winner is null || loser is null)
                throw new InvalidOperationException(
                    $"[{nameof(GameTimeoutBackgroundWorker)}.{nameof(SendTimeoutResultAndSave)}]: game is not ended.");
            var (winnerNewRating, loserNewRating) =
                ratingService.CalculateNewRatingsAfterWin(winner.Rating, loser.Rating);
            await SendResultAndSave(userManager, hubContext, 
                winner, loser,
                GameResultForPlayer.Won, GameResultForPlayer.Lost, 
                $"{loser.Name}'s time is up.", "your time is up.",
                winnerNewRating, loserNewRating);
        }
    }
    
    private async Task SendResultAndSave(UserManager<UserModel> userManager,
        IHubContext<GameHub> hubContext, PlayerInfoModel first, PlayerInfoModel second, 
        GameResultForPlayer firstResult, GameResultForPlayer secondResult,
        string firstReason, string secondReason,
        int firstNewRating, int secondNewRating)
    {
        await UpdateRating(userManager, first.Id, firstNewRating);
        await UpdateRating(userManager, second.Id, secondNewRating);

        var firstPayload = new GameResultPayloadModel(firstResult, firstReason, first.Rating, firstNewRating);
        var secondPayload =
            new GameResultPayloadModel(secondResult, secondReason, second.Rating, secondNewRating);

        await hubContext.Clients.User(first.Id).SendAsync("UpdateGameResult", firstPayload);
        await hubContext.Clients.User(second.Id).SendAsync("UpdateGameResult", secondPayload);
    }
    
    private async Task UpdateRating(UserManager<UserModel> userManager, string userId, int newRating)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return;
        user.Rating = newRating;
        await userManager.UpdateAsync(user);
    }
}