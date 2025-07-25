using CoffeeChess.Domain.Aggregates;

namespace CoffeeChess.Domain.Events;

public record DrawOfferDeclined(string RejectingId, string SenderId) : IDomainEvent;