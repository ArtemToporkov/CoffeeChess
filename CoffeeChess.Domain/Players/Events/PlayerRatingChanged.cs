using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Players.Events;

public record PlayerRatingChanged(string PlayerId, int OldRating, int NewRating) : IDomainEvent;