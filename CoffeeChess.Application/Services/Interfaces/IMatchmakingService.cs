using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Services.Interfaces;

public interface IMatchmakingService
{
    public Task QueueChallenge(string playerId, GameSettings settings);
    
    public Task<bool> TryAddChatMessage(string gameId, string username, string message);
}