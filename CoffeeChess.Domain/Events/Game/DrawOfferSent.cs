namespace CoffeeChess.Domain.Events.Game;

public record DrawOfferSent(string SenderId, string ReceiverId) : IDomainEvent;