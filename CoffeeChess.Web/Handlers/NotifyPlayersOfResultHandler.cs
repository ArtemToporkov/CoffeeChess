using CoffeeChess.Web.Hubs;
using CoffeeChess.Web.Notifications;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Handlers;

public class NotifyPlayersOfResultHandler(IHubContext<GameHub> hubContext) : INotificationHandler<GameResultCalculatedNotification>
{
    public async Task Handle(GameResultCalculatedNotification notification, CancellationToken cancellationToken)
    {
        await hubContext.Clients.User(notification.FirstPlayer.Id)
            .SendAsync("UpdateGameResult", notification.GameResultPayloadForFirst, cancellationToken);
        await hubContext.Clients.User(notification.SecondPlayer.Id)
            .SendAsync("UpdateGameResult", notification.GameResultPayloadForSecond, cancellationToken);
    }
}