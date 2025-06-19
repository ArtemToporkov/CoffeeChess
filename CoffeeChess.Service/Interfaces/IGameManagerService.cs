using CoffeeChess.Core.Models;

namespace CoffeeChess.Service.Interfaces;

public interface IGameManagerService
{
    GameChallengeModel CreateGameChallenge(string creatorId, string creatorUsername, GameSettingsModel settings);
    
    bool TryFindChallenge(string playerId, out GameChallengeModel? foundChallenge);

    public GameModel CreateGameBasedOnFoundChallenge(string playerId,
        GameSettingsModel settings, GameChallengeModel gameChallenge);
    
    bool TryAddChatMessage(string gameId, string username, string message);

    public bool TryGetGame(string gameId, out GameModel? game);

    public bool TryMove(string gameId, string playerId, string from, string to, string? promotion, out GameModel? game);
}