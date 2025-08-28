using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Events;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Chats.ValueObjects;
using CoffeeChess.Infrastructure.Exceptions;
using CoffeeChess.Infrastructure.Serialization;
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
    private const string ChatMetadataSuffix = "metadata";
    
    private static JsonSerializerOptions ChatMessageSerializationOptions => GetChatSerializationOptions();
    
    public async Task<Chat?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var metadata = (await _database.HashGetAllAsync(GetChatMetadataKey(id)))
            .ToDictionary(entry => entry.Name.ToString(), entry => entry.Value);
        if (!metadata.TryGetValue("gameId", out var gameId))
            throw new KeyNotFoundException($"Metadata for {nameof(Chat)} with ID \"{id}\" not found.");
        var chat = new Chat(gameId!);
        foreach (var entry in await _database.ListRangeAsync(GetChatKey(id)))
        {
            var message = JsonSerializer.Deserialize<ChatMessage>(entry!, ChatMessageSerializationOptions);
            // TODO: don't use a reflection
            var messagesField = typeof(Chat).GetField(
                "_messages", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var queue = (ConcurrentQueue<ChatMessage>)messagesField.GetValue(chat)!;
            queue.Enqueue(message);
        }
        return chat;
    }

    public async Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        var metaKey = GetChatMetadataKey(chat.GameId);

        if (await _database.KeyExistsAsync(metaKey))
            throw new KeyAlreadyExistsException(metaKey);
        
        var hashEntries = new HashEntry[]
        {
            new("gameId", chat.GameId)
        };
        await _database.HashSetAsync(metaKey, hashEntries);
    }

    public async Task DeleteAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        // TODO: perform in a transaction
        await _database.KeyDeleteAsync(GetChatKey(chat.GameId));
        await _database.KeyDeleteAsync(GetChatMetadataKey(chat.GameId));
    }

    public async Task SaveChangesAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in chat.DomainEvents)
        {
            if (@event is ChatMessageAdded messageEvent)
            {
                var message = new ChatMessage(messageEvent.Username, messageEvent.Message);
                await _database.ListRightPushAsync(
                    GetChatKey(chat.GameId), JsonSerializer.Serialize(message, ChatMessageSerializationOptions));
            }
            await mediator.Publish(@event, cancellationToken);
        }
        chat.ClearDomainEvents();
    }

    private static string GetChatKey(string id) => $"{ChatKeyPrefix}:{id}";

    private static string GetChatMetadataKey(string id) => $"{ChatKeyPrefix}:{id}:{ChatMetadataSuffix}";

    private static JsonSerializerOptions GetChatSerializationOptions() => new()
    {
        Converters = { new ChatMessageConverter() }
    };
}