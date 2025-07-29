using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Interfaces;

public interface IGameManagerService
{
    public Game? CreateGameOrQueueChallenge(string playerId, GameSettings settings);
    
    bool TryAddChatMessage(string gameId, string username, string message);
}