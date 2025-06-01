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
        var user = await userManager.GetUserAsync(Context.User!);
        return user!.UserName!;
    }
    
    public async Task CreateOrJoinGame(GameSettingsModel settings)
    {
        var username = await GetUsernameAsync();
        if (gameManager.TryFindChallenge(Context.UserIdentifier!, out var foundChallenge))
        {
            var game = gameManager.CreateGameBasedOnFoundChallenge(Context.UserIdentifier!, settings, foundChallenge!);
            await Clients.Users(Context.UserIdentifier!, foundChallenge!.PlayerId).SendAsync(
                "GameStarted", game.GameId, game.WhitePlayerId, game.BlackPlayerId);
        }
        else
        {
            gameManager.CreateGameChallenge(Context.UserIdentifier!, username, settings);
        }
    }
    
    public async Task SendChatMessage(string gameId, string message)
    {
        var username = await GetUsernameAsync();
        if (gameManager.TryGetGame(gameId, out var game) && 
            gameManager.TryAddChatMessage(gameId, username, message))
        {
            await Clients.Users(game!.WhitePlayerId, game.BlackPlayerId)
                .SendAsync("ReceiveChatMessage", username, message + $"{game.WhitePlayerId} {game.BlackPlayerId}");
        }
    }

    public async Task MakeMove(string gameId, string newFen)
    {
        if (gameManager.TryGetGame(gameId, out var game))
        {
            await Clients.Users(game!.WhitePlayerId, game.BlackPlayerId)
                .SendAsync("MakeMove", newFen);
        }
    }
}