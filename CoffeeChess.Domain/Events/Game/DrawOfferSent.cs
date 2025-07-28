namespace CoffeeChess.Domain.Events.Game;

public record DrawOfferSent(string SenderName, string SenderId, string ReceiverId) : IDomainEvent;