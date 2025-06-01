using ChessDotNetCore;
using CoffeeChess.Core.Models;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(IGameManagerService gameManager, UserManager<UserModel> userManager) : Hub
{
    private async Task<string> GetUsernameAsync()
    {
        if (Context.User?.Identity is { IsAuthenticated: true })
        {
            var user = await userManager.GetUserAsync(Context.User);
            return user?.UserName ?? string.Concat("Guest_", Context.ConnectionId.AsSpan(0, 5));
        }
        return string.Concat("Guest_", Context.ConnectionId.AsSpan(0, 5));
    }
    
    public async Task CreateOrJoinGame(GameSettingsModel settings)
    {
        var username = await GetUsernameAsync();
        if (gameManager.TryFindChallenge(Context.ConnectionId, username, settings, out var foundChallenge))
        {
            var game = gameManager.CreateGameBasedOnFoundChallenge(Context.ConnectionId, settings, foundChallenge!);
            await Groups.AddToGroupAsync(foundChallenge!.CreatorConnectionId, game.GameId);
            await Groups.AddToGroupAsync(Context.ConnectionId, game.GameId);
            
            await Clients.Group(game.GameId).SendAsync(
                "GameStarted", game.GameId, game.WhitePlayerId, game.BlackPlayerId);
        }
        else
        {
            var gameChallenge = gameManager.CreateGameChallenge(Context.ConnectionId, username, settings);
            await Clients.Caller.SendAsync("ChallengeCreated", gameChallenge.CreatorConnectionId);
        }
    }
    
    public async Task SendChatMessage(string gameId, string message)
    {
        var username = await GetUsernameAsync();
        if (gameManager.TryAddChatMessage(gameId, username, message))
        {
            await Clients.Group(gameId).SendAsync("ReceiveChatMessage", username, message);
        }
    }

    public async Task MakeMove(string gameId, string newFen)
    {
        if (gameManager.TryGetGame(gameId, out var game))
        {
            await Clients.Group(gameId).SendAsync("MakeMove", newFen);
        }
    }
}