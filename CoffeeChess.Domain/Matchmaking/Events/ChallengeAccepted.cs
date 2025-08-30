using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Matchmaking.Events;

public record ChallengeAccepted(
    string OwnerId, 
    ChallengeSettings AcceptedChallenge, 
    string AcceptorId) : IDomainEvent;