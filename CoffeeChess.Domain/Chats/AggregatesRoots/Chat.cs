using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using CoffeeChess.Domain.Chats.Events;
using CoffeeChess.Domain.Chats.ValueObjects;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Chats.AggregatesRoots;

public class Chat : AggregateRoot<IDomainEvent>
{
    public string GameId { get; }

    public IEnumerable<ChatMessage> Messages => _messages.AsEnumerable();

    private readonly ConcurrentQueue<ChatMessage> _messages;

    public Chat(string gameId)
    {
        GameId = gameId;
        _messages = new();
    }

    public void AddMessage(string username, string message)
    {
        _messages.Enqueue(new(username, message));
        AddDomainEvent(new ChatMessageAdded(GameId, username, message));
    }
}