using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;
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
    private static JsonSerializerOptions ChatSerializationOptions => GetChatSerializationOptions();
    
    public async Task<Chat?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var redisValue = await _database.StringGetAsync($"{ChatKeyPrefix}:{id}");
        if (redisValue.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<Chat>(redisValue!, ChatSerializationOptions);
    }

    public async Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        var serializedChat = JsonSerializer.Serialize(chat, ChatSerializationOptions);
        await _database.StringSetAsync($"{ChatKeyPrefix}:{chat.GameId}", serializedChat, when: When.NotExists);
    }

    public async Task DeleteAsync(Chat chat, CancellationToken cancellationToken = default)
        => await _database.KeyDeleteAsync($"{ChatKeyPrefix}:{chat.GameId}");

    public async Task SaveChangesAsync(Chat chat, CancellationToken cancellationToken = default)
    {
        var serializedGame = JsonSerializer.Serialize(chat, ChatSerializationOptions);
        await _database.StringSetAsync($"{ChatKeyPrefix}:{chat.GameId}", serializedGame);
        
        using var scope = serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        foreach (var @event in chat.DomainEvents)
            await mediator.Publish(@event, cancellationToken);
        chat.ClearDomainEvents();
    }

    private static JsonSerializerOptions GetChatSerializationOptions()
    {
        var jsonTypeResolver = new DefaultJsonTypeInfoResolver();
        jsonTypeResolver.Modifiers.Add(jsonTypeInfo =>
        {
            if (jsonTypeInfo.Type != typeof(Chat))
                return;
            var propsToRemove = jsonTypeInfo.Properties
                .Where(p => p.Name is nameof(Chat.Messages)
                    or nameof(AggregateRoot<IDomainEvent>.DomainEvents))
                .ToList();
            propsToRemove.ForEach(p => jsonTypeInfo.Properties.Remove(p));

            foreach (var field in typeof(Chat).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                         .Where(f => !f.Name.StartsWith('<')))
            {
                var fieldInfo = jsonTypeInfo.CreateJsonPropertyInfo(field.FieldType, field.Name);
                fieldInfo.Get = field.GetValue;
                fieldInfo.Set = field.SetValue;
                jsonTypeInfo.Properties.Add(fieldInfo);
            }
            jsonTypeInfo.CreateObject = () =>
            {
                var chat = (Chat)RuntimeHelpers.GetUninitializedObject(typeof(Chat));
                var domainEventsField = typeof(AggregateRoot<IDomainEvent>).GetField(
                    "_domainEvents", BindingFlags.NonPublic | BindingFlags.Instance) 
                                        ?? throw new SerializationException("Can't find \"_domainEvents\" field.");
                domainEventsField.SetValue(chat, new List<IDomainEvent>());
                return chat;
            };
        });
        return new()
        {
            TypeInfoResolver = jsonTypeResolver
        };
    }
}