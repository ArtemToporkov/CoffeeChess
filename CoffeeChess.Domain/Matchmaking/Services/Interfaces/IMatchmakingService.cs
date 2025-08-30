using CoffeeChess.Domain.Matchmaking.ValueObjects;

namespace CoffeeChess.Domain.Matchmaking.Services.Interfaces;

public interface IMatchmakingService
{
    public Task QueueOrFindChallenge(
        string playerId, ChallengeSettings settings, CancellationToken cancellationToken = default);
}