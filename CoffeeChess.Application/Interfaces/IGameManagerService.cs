using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Interfaces;

public interface IGameManagerService
{
    public Game? CreateGameOrQueueChallenge(Player player, GameSettings settings);
    
    bool TryAddChatMessage(string gameId, string username, string message);
}