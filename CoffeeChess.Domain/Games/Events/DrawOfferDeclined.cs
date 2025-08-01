using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Games.Events;

public record DrawOfferDeclined(string RejectingId, string SenderId) : IDomainEvent;