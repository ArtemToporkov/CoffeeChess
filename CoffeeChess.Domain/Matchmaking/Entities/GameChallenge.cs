using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Matchmaking.ValueObjects;

namespace CoffeeChess.Domain.Matchmaking.Entities;

public class GameChallenge
{
    public string PlayerId { get; init; } = null!;
    public int PlayerRating { get; init; }
    public ChallengeSettings ChallengeSettings { get; init; }

    public GameChallenge(string playerId, int playerRating, ChallengeSettings challengeSettings)
    {
        PlayerId = playerId;
        PlayerRating = playerRating;
        ChallengeSettings = challengeSettings;
    }
}