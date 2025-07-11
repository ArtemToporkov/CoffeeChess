using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Core.Models.Payloads;
using CoffeeChess.Service.Interfaces;
using CoffeeChess.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(
    IGameManagerService gameManager,
    IGameFinisherService gameFinisher,
    UserManager<UserModel> userManager) : Hub<IGameClient>
{
    private async Task<UserModel> GetUserAsync()
        => await userManager.GetUserAsync(Context.User!)
           ?? throw new HubException($"[{nameof(GameHub)}.{nameof(GetUserAsync)}]: User not found.");

    public async Task CreateOrJoinGame(GameSettingsModel settings)
    {
        var user = await GetUserAsync();
        var playerInfo = new PlayerInfoModel(user.Id, user.UserName!, user.Rating);
        var game = gameManager.CreateGameOrQueueChallenge(playerInfo, settings);
        
        if (game is null)
            return;
        
        var totalMillisecondsForOnePlayerLeft = game.WhiteTimeLeft.TotalMilliseconds;

        await Clients.User(game.WhitePlayerInfo.Id).GameStarted(
            game.GameId, true, game.WhitePlayerInfo, game.BlackPlayerInfo,
            totalMillisecondsForOnePlayerLeft);
        await Clients.User(game.BlackPlayerInfo.Id).GameStarted(
            game.GameId, false, game.WhitePlayerInfo, game.BlackPlayerInfo,
            totalMillisecondsForOnePlayerLeft);
    }

    public async Task SendChatMessage(string gameId, string message)
    {
        var user = await GetUserAsync();
        if (gameManager.TryGetGame(gameId, out var game) &&
            gameManager.TryAddChatMessage(gameId, user.UserName!, message))
        {
            await Clients.Users(game.WhitePlayerInfo.Id, game.BlackPlayerInfo.Id)
                .ReceiveChatMessage(user.UserName!, message);
        }
    }

    public async Task MakeMove(string gameId, string from, string to, string? promotion)
    {
        if (!gameManager.TryGetGame(gameId, out var game))
        {
            await Clients.Caller.CriticalError("Game not found");
            return;
        }

        if (game.IsOver)
            await Clients.Caller.MoveFailed( "Game is over.");

        var moveResult = game.MakeMove(Context.UserIdentifier!, from, to, promotion);
        switch (moveResult)
        {
            case MoveResult.Success:
                if (game.PlayerWithDrawOffer.HasValue 
                    && game.GetColorById(Context.UserIdentifier!) != game.PlayerWithDrawOffer.Value)
                {
                    var (sender, receiver) = Context.UserIdentifier! == game.WhitePlayerInfo.Id
                        ? (game.WhitePlayerInfo, game.BlackPlayerInfo)
                        : (game.BlackPlayerInfo, game.WhitePlayerInfo);
                    await SendDrawOfferDeclination(game, sender, receiver);
                }
                var pgn = game.GetPgn();
                await Clients.Users(game.WhitePlayerInfo.Id, game.BlackPlayerInfo.Id).MakeMove(
                    pgn, game.WhiteTimeLeft.TotalMilliseconds, game.BlackTimeLeft.TotalMilliseconds);
                break;
            case MoveResult.Invalid or MoveResult.NotYourTurn:
                await Clients.Caller.MoveFailed(GetMessageByMoveResult(moveResult));
                break;
            case MoveResult.ThreeFold:
            case MoveResult.FiftyMovesRule:
            case MoveResult.Stalemate:
                await gameFinisher.SendDrawResultAndSave(game.WhitePlayerInfo, game.BlackPlayerInfo,
                    GetMessageByMoveResult(moveResult));
                break;
            case MoveResult.Checkmate:
                var (winner, loser) = game.GetWinnerAndLoser();
                if (winner is null || loser is null)
                    throw new InvalidOperationException(
                        $"[{nameof(GameHub)}.{nameof(MakeMove)}]: " +
                        $"{nameof(game)}.{nameof(game.GetWinnerAndLoser)}() does not think the game is ended.");
                await gameFinisher.SendWinResultAndSave(winner, loser,
                    "checkmate.",
                    "checkmate.");
                break;
            case MoveResult.TimeRanOut:
                await Clients.Caller.MoveFailed("Sorry, your time is up.");
                (winner, loser) = game.GetWinnerAndLoser();
                if (winner is null || loser is null)
                    throw new InvalidOperationException(
                        $"[{nameof(GameHub)}.{nameof(MakeMove)}]: " +
                        $"{nameof(game)}.{nameof(game.GetWinnerAndLoser)}() does not think the game is ended.");
                await gameFinisher.SendWinResultAndSave(winner, loser,
                    $"{loser.Name}'s time is up.",
                    $"your time is up.");
                break;
        }
    }

    public async Task PerformGameAction(string gameId, GameActionType gameActionType)
    {
        if (!gameManager.TryGetGame(gameId, out var game))
        {
            await Clients.Caller.CriticalError("Game not found");
            return;
        }

        if (game.IsOver)
        {
            await Clients.Caller.PerformingGameActionFailed("Game is over.");
            return;
        }

        var user = await GetUserAsync();
        var (callerPlayerInfo, receiverPlayerInfo) = user.Id == game.WhitePlayerInfo.Id
            ? (game.WhitePlayerInfo, game.BlackPlayerInfo)
            : (game.BlackPlayerInfo, game.WhitePlayerInfo);
        switch (gameActionType)
        {
            case GameActionType.SendDrawOffer:
                await SendDrawOffer(game, callerPlayerInfo, receiverPlayerInfo);
                break;
            case GameActionType.AcceptDrawOffer:
                game.ClaimDraw();
                await gameFinisher.SendDrawResultAndSave(game.WhitePlayerInfo, game.BlackPlayerInfo, "by agreement.");
                break;
            case GameActionType.DeclineDrawOffer:
                await SendDrawOfferDeclination(game, callerPlayerInfo, receiverPlayerInfo);
                break;
            case GameActionType.Resign:
                await SendResignationResult(game, callerPlayerInfo);
                break;
        }
    }

    private async Task SendDrawOffer(GameModel game, PlayerInfoModel sender, PlayerInfoModel receiver)
    {
        var sendingResult = GetDrawOfferResultOrThrow(game, sender, 
            (gameModel, color) => gameModel.SendDrawOffer(color));
        if (!sendingResult.Success)
        {
            await Clients.User(sender.Id).CriticalError(sendingResult.Message);
            return;
        }
        var offerPayload = new GameActionPayloadModel
        {
            GameActionType = GameActionType.ReceiveDrawOffer,
            Message = $"{sender.Name} offers a draw."
        };
        await Clients.User(receiver.Id).PerformGameAction(offerPayload);
        var sendingPayload = new GameActionPayloadModel { GameActionType = GameActionType.SendDrawOffer };
        await Clients.User(sender.Id).PerformGameAction(sendingPayload);
    }

    private async Task SendDrawOfferDeclination(GameModel game, PlayerInfoModel sender, PlayerInfoModel receiver)
    {
        var declinationResult = GetDrawOfferResultOrThrow(game, sender, 
            (gameModel, color) => gameModel.DeclineDrawOffer(color));
        if (!declinationResult.Success)
        {
            await Clients.User(sender.Id).CriticalError(declinationResult.Message);
            return;
        }
        var declinationPayload = new GameActionPayloadModel { GameActionType = GameActionType.GetDrawOfferDeclination };
        await Clients.User(receiver.Id).PerformGameAction(declinationPayload);
        var declinePayload = new GameActionPayloadModel { GameActionType = GameActionType.DeclineDrawOffer };
        await Clients.User(sender.Id).PerformGameAction(declinePayload);
    }

    private DrawOfferResult GetDrawOfferResultOrThrow(GameModel game, PlayerInfoModel sender, 
        Func<GameModel, PlayerColor, DrawOfferResult> getDrawResult)
    {
        var senderColor = game.GetColorById(sender.Id);
        if (!senderColor.HasValue)
            throw new InvalidOperationException(
                $"[{nameof(GameHub)}.{nameof(GetDrawOfferResultOrThrow)}]: no such player in game {game.GameId}");
        return getDrawResult(game, senderColor.Value);
    }

    private async Task SendResignationResult(GameModel game, PlayerInfoModel caller)
    {
        game.Resign(game.WhitePlayerInfo == caller ? PlayerColor.White : PlayerColor.Black);
        var (winner, loser) = game.GetWinnerAndLoser();
        if (winner is null || loser is null)
            throw new InvalidOperationException(
                $"[{nameof(GameHub)}.{nameof(SendResignationResult)}]: " +
                $"game.{nameof(game.GetWinnerAndLoser)}() does not think the game is ended.");
        await gameFinisher.SendWinResultAndSave(winner, loser,
            $"{caller.Name} resigns.",
            "due to resignation.");
    }

    private string GetMessageByMoveResult(MoveResult moveResult)
        => moveResult switch
        {
            MoveResult.NotYourTurn => "not your turn.",
            MoveResult.TimeRanOut => "time is ran out.",
            MoveResult.Invalid => "invalid move.",
            MoveResult.Success => "success.",
            MoveResult.Checkmate => "checkmate.",
            MoveResult.ThreeFold => "by threefold repetition.",
            MoveResult.FiftyMovesRule => "by 50-move rule.",
            MoveResult.Stalemate => "stalemate.",
            _ => throw new ArgumentException(
                $"[{nameof(GameHub)}.{nameof(GetMessageByMoveResult)}]: unexpected MoveResult.")
        };
}