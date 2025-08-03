using CoffeeChess.Application.Chats.Commands;
using CoffeeChess.Application.Games.Commands;
using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(
    IMediator mediator,
    IMatchmakingService matchmakingService,
    UserManager<UserModel> userManager) : Hub<IGameClient>
{
    private async Task<UserModel> GetUserAsync()
        => await userManager.GetUserAsync(Context.User!)
           ?? throw new HubException($"User {Context.UserIdentifier} not found.");

    public async Task QueueChallenge(GameSettings settings)
    {
        var user = await GetUserAsync();
        await matchmakingService.QueueChallenge(user.Id, settings, Context.ConnectionAborted);
    }

    public async Task SendChatMessage(string gameId, string message)
    {
        var user = await GetUserAsync();
        var sendChatMessageCommand = new SendChatMessageCommand(gameId, user.UserName!, message);
        await mediator.Send(sendChatMessageCommand, Context.ConnectionAborted);
    }

    public async Task MakeMove(string gameId, string from, string to, string? promotion)
    {
        var makeMoveCommand = new MakeMoveCommand(gameId, Context.UserIdentifier!, from, to, promotion);
        await mediator.Send(makeMoveCommand, Context.ConnectionAborted);
    }

    public async Task PerformGameAction(string gameId, GameActionType gameActionType)
    {
        var performGameActionCommand = new PerformGameActionCommand(
            gameId, Context.UserIdentifier!, gameActionType);
        await mediator.Send(performGameActionCommand, Context.ConnectionAborted);
    }
}