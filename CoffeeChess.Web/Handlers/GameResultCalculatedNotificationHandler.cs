using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Handlers;

public class GameResultCalculatedNotificationHandler(IHubContext<GameHub> hubContext) : INotificationHandler<GameResultCalculatedNotification>
{
    public async Task Handle(GameResultCalculatedNotification notification, CancellationToken cancellationToken)
    {
        await hubContext.Clients.User(notification.WhitePlayerInfo.Id)
            .SendAsync("UpdateGameResult", notification.GameResultPayloadForWhite, cancellationToken);
        await hubContext.Clients.User(notification.BlackPlayerInfo.Id)
            .SendAsync("UpdateGameResult", notification.GameResultPayloadForBlack, cancellationToken);
    }
}