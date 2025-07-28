namespace CoffeeChess.Domain.Events.Game;

public record MoveFailed(string MoverId, string Reason) : IDomainEvent;