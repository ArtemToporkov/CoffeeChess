using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record MoveMade(
    string WhiteId, 
    string BlackId, 
    IReadOnlyCollection<string> SanMovesHistory, 
    TimeSpan WhiteTimeLeft, 
    TimeSpan BlackTimeLeft) : IDomainEvent;