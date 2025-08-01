namespace CoffeeChess.Application.Chats.Services.Interfaces;

public interface IChatEventNotifierService
{
    public Task NotifyChatMessageAdded(string whiteId, string blackId, string senderName, string message);
}