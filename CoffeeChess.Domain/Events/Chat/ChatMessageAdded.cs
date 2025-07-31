namespace CoffeeChess.Domain.Events.Chat;

public record ChatMessageAdded(string GameId, string Username, string Message) : IDomainEvent;