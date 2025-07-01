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

        if (game!.IsOver)
            await Clients.Caller.SendAsync("MoveFailed", "Game is over.");

        var moveResult = game.MakeMove(Context.UserIdentifier!, from, to, promotion);
        switch (moveResult)
        {
            case MoveResult.Success:
                var pgn = game.GetPgn();
                await Clients.Users(game.WhitePlayerInfo.Id, game.BlackPlayerInfo.Id).SendAsync(
                    "MakeMove", pgn, game.WhiteTimeLeft.TotalMilliseconds, game.BlackTimeLeft.TotalMilliseconds);
                break;
            case MoveResult.Invalid or MoveResult.TimeRanOut or MoveResult.NotYourTurn:
                await Clients.Caller.SendAsync("MoveFailed", GetMessageByMoveResult(moveResult));
                break;
            case MoveResult.ThreeFold:
                await SendDrawResult(game.WhitePlayerInfo, game.BlackPlayerInfo, "by threefold repetition.");
                break;
            case MoveResult.FiftyMovesRule:
                await SendDrawResult(game.WhitePlayerInfo, game.BlackPlayerInfo, "by 50-move rule.");
                break;
            case MoveResult.Stalemate:
                await SendDrawResult(game.WhitePlayerInfo, game.BlackPlayerInfo, "by stalemate");
                break;
            case MoveResult.Checkmate:
                var (loser, winner) = Context.UserIdentifier == game.WhitePlayerInfo.Id
                    ? (game.BlackPlayerInfo, game.WhitePlayerInfo)
                    : (game.WhitePlayerInfo, game.BlackPlayerInfo);
                await SendWinResult(
                    loser, 
                    winner, 
                    "checkmate.",
                    "checkmate.");
                break;
        }
    }

    public async Task PerformGameAction(string gameId, GameActionType gameActionType)
    {
        if (!gameManager.TryGetGame(gameId, out var game))
        {
            await Clients.Caller.SendAsync("CriticalError", "Game not found");
            return;
        }
        
        if (game!.IsOver)
            await Clients.Caller.SendAsync("PerformingGameActionFailed", "Game is over.");
        
        var user = await GetUserAsync();
        var (callerPlayerInfo, receiverPlayerInfo) = user.Id == game.WhitePlayerInfo.Id
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
                game.ClaimDraw();
                await SendDrawResult(callerPlayerInfo, receiverPlayerInfo, "by agreement.");
                break;
            case GameActionType.DeclineDrawOffer:
                actionPayload = new GameActionPayloadModel
                {
                    GameActionType = GameActionType.GetDrawOfferDeclination,
                };
                await Clients.User(receiverPlayerInfo.Id).SendAsync("PerformGameAction", actionPayload);
                break;
            case GameActionType.Resign:
                game.Resign(game.WhitePlayerInfo == callerPlayerInfo ? PlayerColor.White : PlayerColor.Black);
                await SendWinResult(
                    callerPlayerInfo, 
                    receiverPlayerInfo, 
                    "due to resignation.",
                    $"{callerPlayerInfo.Name} resigns.");
                break;
        }
    }

    private string GetMessageByMoveResult(MoveResult moveResult)
        => moveResult switch
        {
            MoveResult.NotYourTurn => "Not your turn.",
            MoveResult.TimeRanOut => "Time is ran out.",
            MoveResult.Invalid => "Invalid move."
        };

    private async Task SendDrawResult(PlayerInfoModel first, PlayerInfoModel second, string message) 
        => await SendGameResult(first, second, 
            GameResultForPlayer.Draw, GameResultForPlayer.Draw, 
            message, message, 
            0, 0);

    private async Task SendWinResult(PlayerInfoModel loser, PlayerInfoModel winner, 
        string loserMessage, string winnerMessage) 
        => await SendGameResult(loser, winner,
            GameResultForPlayer.Lost, GameResultForPlayer.Won,
            loserMessage, winnerMessage,
            -8, +8);

    private async Task SendGameResult(PlayerInfoModel first, PlayerInfoModel second, 
        GameResultForPlayer resultForFirst, GameResultForPlayer resultForSecond,
        string messageForFirst, string messageForSecond, 
        int firstRatingDelta, int secondRatingDelta)
    {
        var firstPayload = new GameResultPayloadModel
        {
            Result = resultForFirst,
            Message = messageForFirst,
            OldRating = first.Rating,
            NewRating = first.Rating + firstRatingDelta
        };
        var secondPayload = new GameResultPayloadModel
        {
            Result = resultForSecond,
            Message = messageForSecond,
            OldRating = second.Rating,
            NewRating = second.Rating + secondRatingDelta
        };
        await Clients.User(first.Id).SendAsync("UpdateGameResult", firstPayload);
        await Clients.User(second.Id).SendAsync("UpdateGameResult", secondPayload);
    }
}