using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record MoveFailed(string MoverId, MoveFailedReason Reason) : IDomainEvent;