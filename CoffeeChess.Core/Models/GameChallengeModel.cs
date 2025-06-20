using System.Collections.Concurrent;

namespace CoffeeChess.Core.Models;

public class GameChallengeModel(
    PlayerInfoModel playerInfo,
    GameSettingsModel gameSettings) 
{
    public PlayerInfoModel PlayerInfo { get; init; } = playerInfo;
    public GameSettingsModel GameSettings { get; init; } = gameSettings;
}