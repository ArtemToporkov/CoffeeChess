using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Domain.Aggregates;

public class GameChallenge(
    Player player,
    GameSettings gameSettings) 
{
    public Player Player { get; init; } = player;
    public GameSettings GameSettings { get; init; } = gameSettings;
}