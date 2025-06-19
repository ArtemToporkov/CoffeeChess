using System.Diagnostics;
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
        var user = await userManager.GetUserAsync(Context.User!);
        return user!.UserName!;
    }
    
    public async Task CreateOrJoinGame(GameSettingsModel settings)
    {
        var username = await GetUsernameAsync();
        if (gameManager.TryFindChallenge(Context.UserIdentifier!, out var foundChallenge))
        {
            var game = gameManager.CreateGameBasedOnFoundChallenge(Context.UserIdentifier!, settings, foundChallenge!);
            var totalMillisecondsForOnePlayerLeft = game.WhiteTimeLeft.TotalMilliseconds;
            
            await Clients.User(game.WhitePlayerId).SendAsync(
                "GameStarted", game.GameId, true, totalMillisecondsForOnePlayerLeft);
            await Clients.User(game.BlackPlayerId).SendAsync(
                "GameStarted", game.GameId, false, totalMillisecondsForOnePlayerLeft);
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
                .SendAsync("ReceiveChatMessage", username, message);
        }
    }

    public async Task MakeMove(string gameId, string from, string to, string? promotion)
    {
        if (!gameManager.TryMove(gameId, Context.UserIdentifier!, from, to, promotion, out var game))
        {
            await Clients.Caller.SendAsync(
                "MakeMove", game!.ChessGame.GetFen(), false, game.ChessGame.CurrentPlayer == Player.White,
                game.WhiteTimeLeft.TotalMilliseconds, game.BlackTimeLeft.TotalMilliseconds);
            return;
        }

        var newFen = game!.ChessGame.GetFen();
        var isWhiteToMove = game.ChessGame.CurrentPlayer == Player.White;
        
        await Clients.User(game.WhitePlayerId).SendAsync(
            "MakeMove", newFen, isWhiteToMove, isWhiteToMove,
            game.WhiteTimeLeft.TotalMilliseconds, game.BlackTimeLeft.TotalMilliseconds);
        await Clients.User(game.BlackPlayerId).SendAsync(
            "MakeMove", newFen, !isWhiteToMove, isWhiteToMove,
            game.WhiteTimeLeft.TotalMilliseconds, game.BlackTimeLeft.TotalMilliseconds);
    }
}