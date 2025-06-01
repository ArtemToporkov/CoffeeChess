using System.Collections.Concurrent;

namespace CoffeeChess.Core.Models;

public class GameChallengeModel(
    string playerId, 
    string playerUserName,
    GameSettingsModel gameSettings) 
{
    public string PlayerId { get; set; } = playerId;
    public string PlayerUserName { get; set; } = playerUserName;
    public GameSettingsModel GameSettings { get; set; } = gameSettings;
}