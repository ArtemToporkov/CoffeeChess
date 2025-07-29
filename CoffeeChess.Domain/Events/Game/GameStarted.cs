namespace CoffeeChess.Domain.Events.Game;

public record GameStarted(
    string GameId, 
    string WhitePlayerId, 
    string BlackPlayerId, 
    int TotalMillisecondsForOnePlayerLeft) : IDomainEvent;