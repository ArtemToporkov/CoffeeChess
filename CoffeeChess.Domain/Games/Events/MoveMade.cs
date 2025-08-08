using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record MoveMade(
    string WhiteId, 
    string BlackId, 
    IReadOnlyCollection<MoveInfo> MovesHistory, 
    TimeSpan WhiteTimeLeft, 
    TimeSpan BlackTimeLeft) : IDomainEvent;