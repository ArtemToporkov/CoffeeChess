using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record GameStarted(string GameId) : IDomainEvent;