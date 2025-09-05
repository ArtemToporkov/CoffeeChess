using CoffeeChess.Application.Chats.Commands;
using CoffeeChess.Application.Games.Commands;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Infrastructure.Identity;
using CoffeeChess.Web.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Hubs;

public class GameHub(
    IMediator mediator,
    UserManager<UserModel> userManager) : Hub<IGameClient>
{
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

    public override async Task OnConnectedAsync()
    {
        var playerId = Context.UserIdentifier!;
        var checkForActiveGamesCommand = new CheckForActiveGameCommand(playerId);
        var activeGameId = await mediator.Send(checkForActiveGamesCommand);
        if (!string.IsNullOrEmpty(activeGameId))
            await Clients.User(playerId).GameStarted(activeGameId);
        await base.OnConnectedAsync();
    }
    
    private async Task<UserModel> GetUserAsync()
        => await userManager.GetUserAsync(Context.User!)
           ?? throw new UserNotFoundException(Context.UserIdentifier!);
}