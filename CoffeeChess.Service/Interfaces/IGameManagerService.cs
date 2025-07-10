using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Core.Models;

namespace CoffeeChess.Service.Interfaces;

public interface IGameManagerService
{
    public GameModel? CreateGameOrQueueChallenge(PlayerInfoModel player, GameSettingsModel settings);
    
    bool TryAddChatMessage(string gameId, string username, string message);

    public bool TryGetGame(string gameId, [NotNullWhen(true)] out GameModel? game);
    
    public IEnumerable<GameModel> GetActiveGames();
}