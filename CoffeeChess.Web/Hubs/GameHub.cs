using System.Diagnostics;
using ChessDotNetCore;
using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(IGameManagerService gameManager, UserManager<UserModel> userManager) : Hub
{
    private async Task<UserModel> GetUserAsync() 
        => await userManager.GetUserAsync(Context.User!) 
           ?? throw new HubException("[GameHub.GetUserAsync]: User not found.");
    
    public async Task CreateOrJoinGame(GameSettingsModel settings)
    {
        var user = await GetUserAsync();
        var playerInfo = new PlayerInfoModel(user.Id, user.UserName!, user.Rating);
        if (gameManager.TryFindChallenge(playerInfo, out var foundChallenge))
        {
            var game = gameManager.CreateGameBasedOnFoundChallenge(playerInfo, settings, foundChallenge!);
            var totalMillisecondsForOnePlayerLeft = game.WhiteTimeLeft.TotalMilliseconds;
            
            await Clients.User(game.WhitePlayerInfo.Id).SendAsync(
                "GameStarted", game.GameId, true, game.WhitePlayerInfo, game.BlackPlayerInfo, 
                totalMillisecondsForOnePlayerLeft);
            await Clients.User(game.BlackPlayerInfo.Id).SendAsync(
                "GameStarted", game.GameId, false, game.WhitePlayerInfo, game.BlackPlayerInfo, 
                totalMillisecondsForOnePlayerLeft);
        }
        else
        {
            gameManager.CreateGameChallenge(playerInfo, settings);
        }
    }
    
    public async Task SendChatMessage(string gameId, string message)
    {
        var user = await GetUserAsync();
        if (gameManager.TryGetGame(gameId, out var game) && 
            gameManager.TryAddChatMessage(gameId, user.UserName!, message))
        {
            await Clients.Users(game!.WhitePlayerInfo.Id, game.BlackPlayerInfo.Id)
                .SendAsync("ReceiveChatMessage", user.UserName!, message);
        }
    }

    public async Task MakeMove(string gameId, string from, string to, string? promotion)
    {
        if (!gameManager.TryGetGame(gameId, out var game))
        {
            await Clients.Caller.SendAsync("CriticalError", "Game not found");
            return;
        }

        var moveResult = game!.MakeMove(Context.UserIdentifier!, from, to, promotion);
        if (!moveResult.Success)
        {
            await Clients.Caller.SendAsync("MoveFailed", moveResult.Message);
            return;
        }

        var pgn = game.GetPgn();
        
        await Clients.Users(game.WhitePlayerInfo.Id, game.BlackPlayerInfo.Id).SendAsync(
            "MakeMove", pgn, game.WhiteTimeLeft.TotalMilliseconds, game.BlackTimeLeft.TotalMilliseconds);
    }

    public async Task PerformGameAction(string gameId, GameActionType gameActionType)
    {
        if (!gameManager.TryGetGame(gameId, out var game))
        {
            await Clients.Caller.SendAsync("CriticalError", "Game not found");
            return;
        }
        
        var user = await GetUserAsync();
        var (callerPlayerInfo, receiverPlayerInfo) = user.Id == game!.WhitePlayerInfo.Id
            ? (game.WhitePlayerInfo, game.BlackPlayerInfo)
            : (game.BlackPlayerInfo, game.WhitePlayerInfo);
        switch (gameActionType)
        {
            case GameActionType.SendDrawOffer:
                var actionPayload = new GameActionPayloadModel
                {
                    GameActionType = GameActionType.ReceiveDrawOffer,
                    Message = $"{user.UserName} offers a draw."
                };
                await Clients.User(receiverPlayerInfo.Id).SendAsync("PerformGameAction", actionPayload);
                break;
            
            case GameActionType.AcceptDrawOffer:
                await SendGameResultAfterDrawAgreement(callerPlayerInfo, receiverPlayerInfo);
                break;
            case GameActionType.DeclineDrawOffer:
                actionPayload = new GameActionPayloadModel
                {
                    GameActionType = GameActionType.GetDrawOfferDeclination,
                };
                await Clients.User(receiverPlayerInfo.Id).SendAsync("PerformGameAction", actionPayload);
                break;
            case GameActionType.Resign:
                await SendGameResultAfterResignation(callerPlayerInfo, receiverPlayerInfo);
                break;
        }
    }

    private async Task SendGameResultAfterDrawAgreement(PlayerInfoModel first, PlayerInfoModel second)
    {
        var payload = new GameResultPayloadModel
        {
            Result = GameResultForPlayer.Draw,
            Message = "by agreement",
            OldRating = first.Rating,
            NewRating = first.Rating + 0,
        };
        await Clients.User(first.Id).SendAsync("UpdateGameResult", payload);
        payload.OldRating = second.Rating;
        payload.NewRating = second.Rating + 0;
        await Clients.User(second.Id).SendAsync("UpdateGameResult", payload);
    }

    private async Task SendGameResultAfterResignation(PlayerInfoModel loser, PlayerInfoModel winner)
    {
        var loserPayload = new GameResultPayloadModel
        {
            Result = GameResultForPlayer.Lost,
            Message = "due to resignation.",
            OldRating = loser.Rating,
            NewRating = loser.Rating - 8
        };
        var winnerPayload = new GameResultPayloadModel
        {
            Result = GameResultForPlayer.Won,
            Message = $"{loser.Name} resigns.",
            OldRating = winner.Rating,
            NewRating = winner.Rating + 8
        };
        await Clients.User(loser.Id).SendAsync("UpdateGameResult", loserPayload);
        await Clients.User(winner.Id).SendAsync("UpdateGameResult", winnerPayload);
    }
}