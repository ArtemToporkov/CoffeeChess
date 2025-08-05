using System.Text.Json.Serialization;
using CoffeeChess.Domain.Games.ValueObjects;

namespace CoffeeChess.Domain.Players.Entities;

public class GameChallenge
{
    public string PlayerId { get; init; } = null!;
    public GameSettings GameSettings { get; init; }

    public GameChallenge(string playerId, GameSettings gameSettings)
    {
        PlayerId = playerId;
        GameSettings = gameSettings;
    }
}