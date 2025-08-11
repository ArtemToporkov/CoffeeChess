using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record GameEnded(
    string GameId,
    string WhiteId, 
    string BlackId, 
    GameResult GameResult, 
    GameResultReason GameResultReason) : IDomainEvent;