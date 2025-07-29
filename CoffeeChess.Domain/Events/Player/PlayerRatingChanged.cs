namespace CoffeeChess.Domain.Events.Player;

public record PlayerRatingChanged(string PlayerId, int OldRating, int NewRating) : IDomainEvent;