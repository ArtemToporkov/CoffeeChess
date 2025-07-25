namespace CoffeeChess.Domain.Events;

public record MoveFailed(string MoverId, string Reason) : IDomainEvent;