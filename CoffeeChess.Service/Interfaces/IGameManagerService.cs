using CoffeeChess.Core.Models;

namespace CoffeeChess.Service.Interfaces;

public interface IGameManagerService
{
    GameChallengeModel CreateGameChallenge(PlayerInfoModel creatorInfo, GameSettingsModel settings);
    
    bool TryFindChallenge(PlayerInfoModel playerInfo, out GameChallengeModel? foundChallenge);

    public GameModel CreateGameBasedOnFoundChallenge(PlayerInfoModel connectingPlayerInfo,
        GameSettingsModel settings, GameChallengeModel gameChallenge);
    
    bool TryAddChatMessage(string gameId, string username, string message);

    public bool TryGetGame(string gameId, out GameModel? game);
}