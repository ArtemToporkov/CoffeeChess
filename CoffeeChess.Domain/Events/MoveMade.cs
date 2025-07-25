namespace CoffeeChess.Domain.Events;

public record MoveMade(
    string WhiteId, string BlackId, string NewPgn, TimeSpan WhiteTimeLeft, TimeSpan BlackTimeLeft) : IDomainEvent;