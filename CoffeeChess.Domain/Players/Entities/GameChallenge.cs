using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Domain.Players.Entities;

public class GameChallenge(
    string playerId,
    GameSettings gameSettings) 
{
    public string PlayerId { get; init; } = playerId;
    public GameSettings GameSettings { get; init; } = gameSettings;
}