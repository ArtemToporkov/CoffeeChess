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

    public Task<Chat?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _ = _chats.TryGetValue(id, out var chat);
        return Task.FromResult(chat);
    }

    public Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        _chats.TryAdd(chat.GameId, chat);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        _chats.TryRemove(chat.GameId, out _);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        // TODO: use redis instead of in-memory implementation
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in chat.DomainEvents)
            await mediator.Publish(@event);
        chat.ClearDomainEvents();
    }
}