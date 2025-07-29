using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Events.Game;

public record MoveFailed(string MoverId, MoveFailedReason Reason) : IDomainEvent;