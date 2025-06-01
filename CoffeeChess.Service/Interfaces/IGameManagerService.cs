using CoffeeChess.Core.Models;

namespace CoffeeChess.Service.Interfaces;

public interface IGameManagerService
{
    GameChallengeModel CreateGameChallenge(string creatorConnectionId, string creatorUsername, GameSettingsModel settings);
    
    bool TryFindChallenge(string playerConnectionId, string playerUsername, 
        GameSettingsModel settings, out GameChallengeModel? foundChallenge);

    public GameModel CreateGameBasedOnFoundChallenge(string playerConnectionId,
        GameSettingsModel settings, GameChallengeModel gameChallenge);
    
    bool TryAddChatMessage(string gameId, string username, string message);

    public bool TryGetGame(string gameId, out GameModel? game);
}