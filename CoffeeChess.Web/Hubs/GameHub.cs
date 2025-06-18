using System.Diagnostics;
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
            await Clients.User(game.WhitePlayerId).SendAsync(
                "GameStarted", game.GameId, true);
            await Clients.User(game.BlackPlayerId).SendAsync(
                "GameStarted", game.GameId, false);
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

    public async Task MakeMove(string gameId, string oldFen, string newFen)
    {
        if (gameManager.TryGetGame(gameId, out var game))
        {
            if (game!.WhitePlayerId == Context.UserIdentifier)
            {
                if (game.IsWhiteTurn)
                {
                    game.IsWhiteTurn = false;
                    await Clients.User(game.WhitePlayerId).SendAsync(
                        "MakeMove", newFen, false);
                    await Clients.User(game.BlackPlayerId).SendAsync(
                        "MakeMove", newFen, true);
                }
                else
                    await Clients.User(game.WhitePlayerId).SendAsync(
                        "MakeMove", oldFen, false);
            }
            else if (game.BlackPlayerId == Context.UserIdentifier)
            {
                if (!game.IsWhiteTurn)
                {
                    game.IsWhiteTurn = true;
                    await Clients.User(game.WhitePlayerId).SendAsync(
                        "MakeMove", newFen, true);
                    await Clients.User(game.BlackPlayerId).SendAsync(
                        "MakeMove", newFen, false);
                }
                else
                    await Clients.User(game.BlackPlayerId).SendAsync(
                        "MakeMove", oldFen, false);
            }
        }
    }
}