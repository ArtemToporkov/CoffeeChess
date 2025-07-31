namespace CoffeeChess.Domain.Events.Chat;

public record ChatMessageAdded(string Username, string Message) : IDomainEvent;