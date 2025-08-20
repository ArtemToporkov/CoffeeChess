using CoffeeChess.Domain.Matchmaking.ValueObjects;

namespace CoffeeChess.Domain.Matchmaking.Entities;

public class GameChallenge(string playerId, int playerRating, ChallengeSettings challengeSettings)
{
    public string PlayerId { get; } = playerId;
    public int PlayerRating { get; } = playerRating;
    public ChallengeSettings ChallengeSettings { get; } = challengeSettings;
}