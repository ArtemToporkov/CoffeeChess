using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Domain.Entities;

public class GameChallenge(
    Player player,
    GameSettings gameSettings) 
{
    public Player Player { get; init; } = player;
    public GameSettings GameSettings { get; init; } = gameSettings;
}