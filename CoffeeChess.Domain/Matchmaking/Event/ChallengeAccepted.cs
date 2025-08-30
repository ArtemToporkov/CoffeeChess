using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Matchmaking.Event;

public record ChallengeAccepted(string OwnerId, string AcceptorId) : IDomainEvent;