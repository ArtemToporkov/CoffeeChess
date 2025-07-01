using CoffeeChess.Core.Enums;
using CoffeeChess.Service.Interfaces;

namespace CoffeeChess.Service.Implementations;

public class EloRatingService : IRatingService
{
    private static int KFactor { get; set; } = 15;

    public (int NewWhiteRating, int NewBlackRating) CalculateNewRatings(
        int whiteRating, int blackRating, Result result)
    {
        var whitePoints = result switch
        {
            Result.WhiteWins => 1.0,
            Result.BlackWins => 0.0,
            Result.Draw => 0.5,
            _ => throw new ArgumentException($"[EloRatingService.CalculateNewRatings]: " +
                                             $"unexpected argument for {nameof(result)}.")
        };
        var blackPoints = 1 - whitePoints;
        return (GetRatingForPlayer(whiteRating, blackRating, whitePoints),
            GetRatingForPlayer(blackRating, whiteRating, blackPoints));
    }

    private static int GetRatingForPlayer(int playersRating, int opponentsRating, double playersPoints)
    {
        var expectedPoints = 1.0 / (1 + Math.Pow(10, ((double)opponentsRating - playersRating) / 400));
        var rawDelta = KFactor * (playersPoints - expectedPoints);
        var roundedDelta = (int)Math.Round(rawDelta, MidpointRounding.AwayFromZero);
        
        return playersRating + roundedDelta;
    }
}