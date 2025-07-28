namespace CoffeeChess.Domain.Events.Game;

public record DrawOfferDeclined(string RejectingId, string SenderId) : IDomainEvent;