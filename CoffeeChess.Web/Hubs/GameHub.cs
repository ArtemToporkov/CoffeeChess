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

    public async Task MakeMove(string gameId, string from, string to, string? promotion)
    {
        if (gameManager.TryGetGame(gameId, out var game))
        {
            var isWhiteToMove = game!.ChessGame.CurrentPlayer == Player.White;
            var currentPlayerId = isWhiteToMove ? game.WhitePlayerId : game.BlackPlayerId;
            
            if (Context.UserIdentifier != currentPlayerId)
            {
                await Clients.User(currentPlayerId).SendAsync(
                    "MakeMove", game.ChessGame.GetFen(), false);
                return;
            }

            var promotionChar = promotion?[0];
            var move = new Move(new(from), new(to), game.ChessGame.CurrentPlayer, promotionChar);

            if (game.ChessGame.MakeMove(move, true) is MoveType.Invalid)
            {
                await Clients.User(currentPlayerId).SendAsync(
                    "MakeMove", game.ChessGame.GetFen(), false);
                return;
            }

            isWhiteToMove = !isWhiteToMove;
            var newFen = game.ChessGame.GetFen();
            await Clients.User(game.WhitePlayerId).SendAsync(
                "MakeMove", newFen, isWhiteToMove);
            await Clients.User(game.BlackPlayerId).SendAsync(
                "MakeMove", newFen, !isWhiteToMove);
        }
    }
}