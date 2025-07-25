using CoffeeChess.Application.Interfaces;
using CoffeeChess.Application.Payloads;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Domain.ValueObjects;
using CoffeeChess.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(
    IGameRepository gameRepository,
    IGameManagerService gameManager,
    IGameFinisherService gameFinisher,
    UserManager<UserModel> userManager) : Hub<IGameClient>, IGameEventNotifier
{
    private async Task<UserModel> GetUserAsync()
        => await userManager.GetUserAsync(Context.User!)
           ?? throw new HubException($"[{nameof(GameHub)}.{nameof(GetUserAsync)}]: User not found.");

    public async Task CreateOrJoinGame(GameSettings settings)
    {
        var user = await GetUserAsync();
        var playerInfo = new PlayerInfo(user.Id, user.UserName!, user.Rating);
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
        if (gameRepository.TryGetValue(gameId, out var game) &&
            gameManager.TryAddChatMessage(gameId, user.UserName!, message))
        {
            await Clients.Users(game.WhitePlayerInfo.Id, game.BlackPlayerInfo.Id)
                .ReceiveChatMessage(user.UserName!, message);
        }
    }

    public async Task MakeMove(string gameId, string from, string to, string? promotion)
    {
        if (!gameRepository.TryGetValue(gameId, out var game))
        {
            await Clients.Caller.CriticalError("Game not found");
            return;
        }

        if (game.IsOver)
            await Clients.Caller.MoveFailed( "Game is over.");

        game.ApplyMove(Context.UserIdentifier!, from, to, promotion);
        gameRepository.SaveChanges(game);
    }

    public async Task PerformGameAction(string gameId, GameActionType gameActionType)
    {
        if (!gameRepository.TryGetValue(gameId, out var game))
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
        switch (gameActionType)
        {
            case GameActionType.SendDrawOffer:
                game.OfferADraw(user.Id);
                gameRepository.SaveChanges(game);
                break;
            case GameActionType.AcceptDrawOffer:
                game.AcceptDrawOffer(user.Id);
                gameRepository.SaveChanges(game);
                break;
            case GameActionType.DeclineDrawOffer:
                game.DeclineDrawOffer(user.Id);
                gameRepository.SaveChanges(game);
                break;
            case GameActionType.Resign:
                game.Resign(user.Id);
                gameRepository.SaveChanges(game);
                break;
        }
    }

    public async Task NotifyMoveMade(string whiteId, 
        string blackId, string pgn, double whiteTimeLeft, double blackTimeLeft)
    {
        await Clients.Users(whiteId, blackId).MakeMove(pgn, whiteTimeLeft, blackTimeLeft);
    }

    public async Task NotifyMoveFailed(string moverId, string reason)
    {
        await Clients.User(moverId).MoveFailed(reason);
    }

    public async Task NotifyGameResultUpdated(PlayerInfo whiteInfo, PlayerInfo blackInfo, Result result, 
        string whiteReason, string blackReason)
    {
        switch (result)
        {
            case Result.WhiteWon:
                await gameFinisher.SendWinResultAndSave(whiteInfo, blackInfo, whiteReason, blackReason);
                break;
            case Result.BlackWon:
                await gameFinisher.SendWinResultAndSave(blackInfo, whiteInfo, blackReason, whiteReason);
                break;
            case Result.Draw:
                await gameFinisher.SendDrawResultAndSave(whiteInfo, blackInfo, whiteReason);
                break;
        }
    }

    public async Task NotifyDrawOfferSent(string senderName, string senderId, string receiverId)
    {
        var offerPayload = new GameActionPayloadModel
        {
            GameActionType = GameActionType.ReceiveDrawOffer,
            Message = $"{senderName} offers a draw."
        };
        await Clients.User(receiverId).PerformGameAction(offerPayload);
        var sendingPayload = new GameActionPayloadModel { GameActionType = GameActionType.SendDrawOffer };
        await Clients.User(senderId).PerformGameAction(sendingPayload);
    }

    public async Task NotifyDrawOfferDeclined(string rejectingId, string senderId)
    {
        var senderPayload = new GameActionPayloadModel { GameActionType = GameActionType.GetDrawOfferDeclination };
        await Clients.User(senderId).PerformGameAction(senderPayload);
        var rejectingPayload = new GameActionPayloadModel { GameActionType = GameActionType.DeclineDrawOffer };
        await Clients.User(rejectingId).PerformGameAction(rejectingPayload);
    }
}