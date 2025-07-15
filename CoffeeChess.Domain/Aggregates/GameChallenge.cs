using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Domain.Aggregates;

public class GameChallenge(
    PlayerInfo playerInfo,
    GameSettings gameSettings) 
{
    public PlayerInfo PlayerInfo { get; init; } = playerInfo;
    public GameSettings GameSettings { get; init; } = gameSettings;
}