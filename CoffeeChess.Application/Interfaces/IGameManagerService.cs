using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Interfaces;

public interface IGameManagerService
{
    public Game? CreateGameOrQueueChallenge(PlayerInfo player, GameSettings settings);
    
    bool TryAddChatMessage(string gameId, string username, string message);

    public bool TryGetGame(string gameId, [NotNullWhen(true)] out Game? game);
    
    public IEnumerable<Game> GetActiveGames();
}