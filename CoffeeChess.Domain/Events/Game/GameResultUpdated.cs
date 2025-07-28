using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Events.Game;

public record GameResultUpdated(Aggregates.Player White, Aggregates.Player Black, Result Result, 
    string WhiteReason, string BlackReason) : IDomainEvent;