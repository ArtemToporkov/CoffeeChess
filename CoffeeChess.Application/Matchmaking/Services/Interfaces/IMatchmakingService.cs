using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Matchmaking.ValueObjects;

namespace CoffeeChess.Application.Matchmaking.Services.Interfaces;

public interface IMatchmakingService
{
    public Task QueueOrFindChallenge(
        string playerId, ChallengeSettings settings, CancellationToken cancellationToken = default);
}