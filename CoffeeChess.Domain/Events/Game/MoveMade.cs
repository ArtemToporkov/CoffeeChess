namespace CoffeeChess.Domain.Events.Game;

public record MoveMade(
    string WhiteId, 
    string BlackId, 
    IReadOnlyCollection<string> SanMovesHistory, 
    TimeSpan WhiteTimeLeft, 
    TimeSpan BlackTimeLeft) : IDomainEvent;