namespace CoffeeChess.Domain.Events.Game;

public record MoveMade(
    string WhiteId, string BlackId, string NewPgn, TimeSpan WhiteTimeLeft, TimeSpan BlackTimeLeft) : IDomainEvent;