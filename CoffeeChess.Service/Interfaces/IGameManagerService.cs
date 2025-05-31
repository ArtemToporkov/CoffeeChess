using CoffeeChess.Core.Models;

namespace CoffeeChess.Service.Interfaces;

public interface IGameManagerService
{
    GameModel CreateGame(string creatorConnectionId, string creatorUsername, GameSettingsModel settings);
    bool TryJoinGame(string gameId, string joinerConnectionId, string joinerUsername, out GameModel? joinedGame);
    bool TryGetGame(string gameId, out GameModel? game);
    bool TryAddChatMessage(string gameId, string username, string message);
    GameModel FindOrCreateGame(string playerConnectionId, string playerUsername, GameSettingsModel settings);
}