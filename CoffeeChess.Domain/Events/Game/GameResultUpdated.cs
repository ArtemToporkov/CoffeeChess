using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Events.Game;

public record GameResultUpdated(
    string WhiteId, 
    string BlackId, 
    GameResult GameResult, 
    GameResultReason GameResultReason) : IDomainEvent;