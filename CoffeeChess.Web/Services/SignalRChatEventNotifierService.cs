using CoffeeChess.Application.Chats.Services.Interfaces;
using CoffeeChess.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CoffeeChess.Web.Services;

public class SignalRChatEventNotifierService(
    IHubContext<GameHub, IGameClient> hubContext) : IChatEventNotifierService
{
    public async Task NotifyChatMessageAdded(string whiteId, string blackId, string senderName, string message, 
        CancellationToken cancellationToken = default)
    {
        await hubContext.Clients.Users(whiteId, blackId).ChatMessageReceived(
           senderName, message, cancellationToken);
    }
}