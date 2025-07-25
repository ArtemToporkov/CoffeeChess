using CoffeeChess.Domain.Aggregates;
using MediatR;

namespace CoffeeChess.Domain.Events;

public record DrawOfferSent(string SenderName, string SenderId, string ReceiverId) : IDomainEvent;