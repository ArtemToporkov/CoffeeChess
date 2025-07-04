using System.Diagnostics;
using ChessDotNetCore;
using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Models;
using CoffeeChess.Web.Notifications;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(
    IGameManagerService gameManager,
    UserManager<UserModel> userManager,
    IRatingService ratingService,
    IMediator mediator) : Hub
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
            case MoveResult.Invalid or MoveResult.NotYourTurn:
                await Clients.Caller.SendAsync("MoveFailed", GetMessageByMoveResult(moveResult));
                break;
            case MoveResult.ThreeFold:
                await PublishDrawResult(game.WhitePlayerInfo, game.BlackPlayerInfo, "by threefold repetition.");
                break;
            case MoveResult.FiftyMovesRule:
                await PublishDrawResult(game.WhitePlayerInfo, game.BlackPlayerInfo, "by 50-move rule.");
                break;
            case MoveResult.Stalemate:
                await PublishDrawResult(game.WhitePlayerInfo, game.BlackPlayerInfo, "by stalemate");
                break;
            case MoveResult.Checkmate:
                var (winner, loser) = game.GetWinnerAndLoser();
                if (winner is null || loser is null)
                    throw new InvalidOperationException(
                        "[GameHub.MakeMove]: game.GetWinnerAndLoser() does not think the game is ended.");
                await PublishWinResult(winner, loser,
                    "checkmate.",
                    "checkmate.");
                break;
            case MoveResult.TimeRanOut:
                (winner, loser) = game.GetWinnerAndLoser();
                if (winner is null || loser is null)
                    throw new InvalidOperationException(
                        "[GameHub.MakeMove]: game.GetWinnerAndLoser() does not think the game is ended.");
                await PublishWinResult(winner, loser,
                    $"{loser.Name}'s time is up.",
                    $"your time is up.");
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
                await PublishDrawResult(game.WhitePlayerInfo, game.BlackPlayerInfo, "by agreement.");
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
                var (winner, loser) = game.GetWinnerAndLoser();
                if (winner is null || loser is null)
                    throw new InvalidOperationException(
                        $"[{nameof(GameHub)}.{nameof(PerformGameAction)}]: " +
                        $"game.{nameof(game.GetWinnerAndLoser)} does not think the game is ended.");
                await PublishWinResult(winner, loser,
                    $"{callerPlayerInfo.Name} resigns.",
                    "due to resignation.");
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

    private async Task PublishDrawResult(PlayerInfoModel first, PlayerInfoModel second, string reason)
    {
        await mediator.Publish(new GameDrawnNotification
        {
            FirstPlayer = first,
            SecondPlayer = second,
            DrawReason = reason
        });
    }

    private async Task PublishWinResult(PlayerInfoModel winner, PlayerInfoModel loser,
        string winReason, string loseReason)
    {
        await mediator.Publish(new GameEndedNotification
        {
            Winner = winner,
            Loser = loser,
            WinReason = winReason,
            LoseReason = loseReason
        });
    }
}