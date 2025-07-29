using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Domain.Entities;

public class GameChallenge(
    string playerId,
    GameSettings gameSettings) 
{
    public string PlayerId { get; init; } = playerId;
    public GameSettings GameSettings { get; init; } = gameSettings;
}