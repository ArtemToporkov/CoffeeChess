using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Events;

public record GameResultUpdated(
    PlayerInfo WhiteInfo, PlayerInfo BlackInfo, Result Result, string WhiteReason, string BlackReason) : IDomainEvent;