using System.Collections.Concurrent;
using CoffeeChess.Domain.Chats.Events;
using CoffeeChess.Domain.Chats.ValueObjects;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Chats.AggregatesRoots;

public class Chat(string gameId) : AggregateRoot<IDomainEvent>
{
    public string GameId { get; } = gameId;
    
    public IEnumerable<ChatMessage> Messages => _messages.AsEnumerable();
    
    private readonly ConcurrentQueue<ChatMessage> _messages = new();

    public void AddMessage(string username, string message)
    {
        _messages.Enqueue(new(username, message, DateTime.UtcNow));
        AddDomainEvent(new ChatMessageAdded(GameId, username, message));
    }
}