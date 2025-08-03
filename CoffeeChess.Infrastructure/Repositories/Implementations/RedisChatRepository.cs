using System.Text.Json;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class RedisChatRepository(
    IServiceProvider serviceProvider,
    IConnectionMultiplexer redis) : IChatRepository
{
    private readonly IDatabase _database = redis.GetDatabase();
    private const string ChatKeyPrefix = "chat";
    
    public async Task<Chat?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var redisValue = await _database.StringGetAsync($"{ChatKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Chat>(redisValue!);
    }

    public async Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        var serializedChat = JsonSerializer.Serialize(chat);
        await _database.StringSetAsync($"{ChatKeyPrefix}:{chat.GameId}", serializedChat, when: When.NotExists);
    }

    public async Task DeleteAsync(Chat chat, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{ChatKeyPrefix}:{chat.GameId}");

    public async Task SaveChangesAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(chat);
        await _database.StringSetAsync($"{ChatKeyPrefix}:{chat.GameId}", serializedGame);
        
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in chat.DomainEvents)
            await mediator.Publish(@event, cancellationToken);
        chat.ClearDomainEvents();
    }
}