namespace CoffeeChess.Domain.Events.Player;

public record RatingChanged(string Id, int OldRating, int NewRating) : IDomainEvent;