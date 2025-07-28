using CoffeeChess.Application.Interfaces;
using CoffeeChess.Application.Payloads;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Services.Interfaces;
using CoffeeChess.Infrastructure.Identity;
using CoffeeChess.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Services;

public class SignalRGameFinisherService(IHubContext<GameHub, IGameClient> hubContext, 
    UserManager<UserModel> userManager, IRatingService ratingService) : IGameFinisherService
{
    public async Task SendDrawResultAndSave(PlayerInfo first, PlayerInfo second, string reason)
    {
        var (firstNewRating, secondNewRating) = ratingService
            .CalculateNewRatingsAfterDraw(first.Rating, second.Rating);
        await SendResultAndSave(first, second,
            GameResultForPlayer.Draw, GameResultForPlayer.Draw,
            reason, reason,
            firstNewRating, secondNewRating);
    }

    public async Task SendWinResultAndSave(PlayerInfo winner, PlayerInfo loser,
        string winReason, string loseReason)
    {
        var (winnerNewRating, loserNewRating) = ratingService.CalculateNewRatingsAfterWin(winner.Rating, loser.Rating);
        await SendResultAndSave(winner, loser, 
            GameResultForPlayer.Won, GameResultForPlayer.Lost,
            winReason, loseReason, 
            winnerNewRating, loserNewRating);
    }

    private async Task SendResultAndSave(PlayerInfo first, PlayerInfo second, 
        GameResultForPlayer firstResult, GameResultForPlayer secondResult,
        string firstReason, string secondReason,
        int firstNewRating, int secondNewRating)
    {
        await UpdateRating(first.Id, firstNewRating);
        await UpdateRating(second.Id, secondNewRating);

        var firstPayload = new GameResultPayloadModel(firstResult, firstReason, first.Rating, firstNewRating);
        var secondPayload =
            new GameResultPayloadModel(secondResult, secondReason, second.Rating, secondNewRating);

        await hubContext.Clients.User(first.Id).UpdateGameResult(firstPayload);
        await hubContext.Clients.User(second.Id).UpdateGameResult(secondPayload);
    }

    private async Task UpdateRating(string userId, int newRating)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return;
        user.Rating = newRating;
        await userManager.UpdateAsync(user);
    }
}