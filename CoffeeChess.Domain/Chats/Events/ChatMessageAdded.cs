using CoffeeChess.Domain.Shared.Interfaces;
using MediatR;

namespace CoffeeChess.Domain.Chats.Events;

public record ChatMessageAdded(string GameId, string Username, string Message) : IDomainEvent, IRequest;