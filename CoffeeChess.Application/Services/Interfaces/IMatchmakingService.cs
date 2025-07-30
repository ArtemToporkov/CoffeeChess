using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Services.Interfaces;

public interface IMatchmakingService
{
    public void QueueChallenge(string playerId, GameSettings settings);
    
    bool TryAddChatMessage(string gameId, string username, string message);
}