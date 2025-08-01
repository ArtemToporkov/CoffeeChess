using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record DrawOfferSent(string SenderId, string ReceiverId) : IDomainEvent;