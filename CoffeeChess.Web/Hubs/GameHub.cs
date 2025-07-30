using CoffeeChess.Application.Services.Interfaces;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Domain.ValueObjects;
using CoffeeChess.Infrastructure.Identity;
using CoffeeChess.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(
    IGameRepository gameRepository,
    IPlayerRepository playerRepository,
    IMatchmakingService matchmakingService,
    UserManager<UserModel> userManager) : Hub<IGameClient>
{
    private async Task<UserModel> GetUserAsync()
        => await userManager.GetUserAsync(Context.User!)
           ?? throw new HubException($"[{nameof(GameHub)}.{nameof(GetUserAsync)}]: User not found.");

    public async Task CreateOrJoinGame(GameSettings settings)
    {
        var user = await GetUserAsync();
        matchmakingService.QueueChallenge(user.Id, settings);
    }

    public async Task SendChatMessage(string gameId, string message)
    {
        var user = await GetUserAsync();
        if (gameRepository.TryGetValue(gameId, out var game) &&
            matchmakingService.TryAddChatMessage(gameId, user.UserName!, message))
        {
            await Clients.Users(game.WhitePlayerId, game.BlackPlayerId)
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

    private async Task<PlayerInfoViewModel> GetInfoAsync(string playerId)
    {
        var player = await playerRepository.GetAsync(playerId) 
                     ?? throw new InvalidOperationException(
                         $"[{nameof(GameHub)}.{nameof(GetInfoAsync)}]: player not found.");
        return new PlayerInfoViewModel(player.Name, player.Rating);
    }
}