using System.Collections.Concurrent;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Domain.Entities;

public class Chat
{
    public IEnumerable<ChatMessage> Messages => _messages.AsEnumerable();
    private readonly ConcurrentQueue<ChatMessage> _messages = new();

    public Task AddMessage(string username, string message)
    {
        _messages.Enqueue(new(username, message, DateTime.UtcNow));
        return Task.CompletedTask;
    }
}