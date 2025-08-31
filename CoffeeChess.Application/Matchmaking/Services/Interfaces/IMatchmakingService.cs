using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.ValueObjects;

namespace CoffeeChess.Application.Matchmaking.Services.Interfaces;

public interface IMatchmakingService
{
    // NOTE: implementations of this method only find matching challenge,
    // they do not send found challenge to other services nor add challenge to database
    public Task<bool> QueueOrFindMatchingChallenge(
        string playerId, int playerRating, ChallengeSettings settings, CancellationToken cancellationToken = default);
    
    public Task AddAsync(Challenge challenge);
}