using CoffeeChess.Domain.Chats.ValueObjects;

namespace CoffeeChess.Application.Chats.ReadModels;

public class ChatHistoryReadModel
{
    public string GameId { get; init; }
    public IReadOnlyList<ChatMessage> Messages { get; init; }
}