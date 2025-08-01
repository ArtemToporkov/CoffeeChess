using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record GameStarted(
    string GameId, 
    string WhitePlayerId, 
    string BlackPlayerId, 
    int TotalMillisecondsForOnePlayerLeft) : IDomainEvent;