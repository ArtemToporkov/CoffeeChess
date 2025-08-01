using System.Collections.Concurrent;
using CoffeeChess.Domain.Chats.Events;
using CoffeeChess.Domain.Chats.ValueObjects;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Chats.AggregatesRoots;

public class Chat(string gameId)
{
    public string GameId { get; } = gameId;
    
    public IEnumerable<ChatMessage> Messages => _messages.AsEnumerable();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    private readonly ConcurrentQueue<ChatMessage> _messages = new();
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public void ClearDomainEvents() => _domainEvents.Clear();

    public Task AddMessage(string username, string message)
    {
        _messages.Enqueue(new(username, message, DateTime.UtcNow));
        _domainEvents.Add(new ChatMessageAdded(GameId, username, message));
        return Task.CompletedTask;
    }
}