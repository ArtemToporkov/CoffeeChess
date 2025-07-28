using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Events;

public record GameResultUpdated(Player White, Player Black, Result Result, 
    string WhiteReason, string BlackReason) : IDomainEvent;