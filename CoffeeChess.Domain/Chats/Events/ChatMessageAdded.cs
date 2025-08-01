using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Chats.Events;

public record ChatMessageAdded(string GameId, string Username, string Message) : IDomainEvent;