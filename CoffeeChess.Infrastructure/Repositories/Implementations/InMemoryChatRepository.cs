using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class InMemoryChatRepository(IServiceProvider serviceProvider) : IChatRepository
{
    private readonly ConcurrentDictionary<string, Chat> _chats = new();

    public bool TryGetValue(string id, [NotNullWhen(true)] out Chat? chat)
    {
        if (_chats.TryGetValue(id, out chat))
            return true;

        chat = null;
        return false;
    }

    public bool TryAdd(string id, Chat chat) => _chats.TryAdd(id, chat);
    
    public bool TryRemove(string id, [NotNullWhen(true)] out Chat? removedChat) 
        => _chats.TryRemove(id, out removedChat);

    public IEnumerable<(string, Chat)> GetAll() 
        => _chats.Select(kvp => (kvp.Key, kvp.Value));

    public void SaveChanges(Chat chat)
    {
    }

    public async Task SaveChangesAsync(Chat chat)
    {
        // TODO: use redis instead of in-memory implementation
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in chat.DomainEvents)
            await mediator.Publish(@event);
        chat.ClearDomainEvents();
    }
}