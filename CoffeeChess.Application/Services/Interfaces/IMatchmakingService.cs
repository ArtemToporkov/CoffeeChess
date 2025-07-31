using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Services.Interfaces;

public interface IMatchmakingService
{
    public Task QueueChallenge(string playerId, GameSettings settings);
}