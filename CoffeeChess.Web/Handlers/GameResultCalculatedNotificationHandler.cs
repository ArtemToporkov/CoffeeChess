using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Handlers;

public class GameResultCalculatedNotificationHandler(IHubContext<GameHub> hubContext) : INotificationHandler<GameResultCalculatedNotification>
{
    public async Task Handle(GameResultCalculatedNotification notification, CancellationToken cancellationToken)
    {
        await hubContext.Clients.User(notification.Game.WhitePlayerInfo.Id)
            .SendAsync("UpdateGameResult", notification.GameResultPayloadForWhite);
        await hubContext.Clients.User(notification.Game.BlackPlayerInfo.Id)
            .SendAsync("UpdateGameResult", notification.GameResultPayloadForBlack);
    }
}