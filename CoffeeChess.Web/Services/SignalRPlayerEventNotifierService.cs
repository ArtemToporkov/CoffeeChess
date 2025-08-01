using CoffeeChess.Application.Players.Services.Interfaces;
using CoffeeChess.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Services;

public class SignalRPlayerEventNotifierService(
    IHubContext<GameHub, IGameClient> hubContext) : IPlayerEventNotifierService
{
    public async Task NotifyPlayerRatingChanged(string playerId, int oldRating, int newRating)
    {
        await hubContext.Clients.User(playerId).PlayerRatingUpdated(oldRating, newRating);
    }
}