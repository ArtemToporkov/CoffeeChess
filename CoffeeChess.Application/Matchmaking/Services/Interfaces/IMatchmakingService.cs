using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Application.Matchmaking.Services.Interfaces;

public interface IMatchmakingService
{
    public Task QueueChallenge(string playerId, GameSettings settings);
}