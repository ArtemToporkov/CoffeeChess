using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Services.Interfaces;

namespace CoffeeChess.Domain.Services.Implementations;

public class EloRatingService : IRatingService
{
    private static int KFactor { get; set; } = 15;

    public (int NewFirstRating, int NewSecondRating) CalculateNewRatingsAfterDraw(int firstRating, int secondRating)
        => CalculateNewRatings(firstRating, secondRating, 0.5);
    
    public (int NewWinnerRating, int NewLoserRating) CalculateNewRatingsAfterWin(int winnerRating, int loserRating)
        => CalculateNewRatings(winnerRating, loserRating, 1.0);

    public (int NewWhiteRating, int NewBlackRating) CalculateNewRatings(int whiteRating, int blackRating,
        Result result)
    {
        var expectedWhitePoints = 1.0 / (1 + Math.Pow(10, ((double)blackRating - whiteRating) / 400));
        var actualWhitePoints = result switch
        {
            Result.WhiteWon => 1.0,
            Result.Draw => 0.5,
            Result.BlackWon => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
        
        var rawDelta = KFactor * (actualWhitePoints - expectedWhitePoints);
        var roundedDelta = (int)Math.Round(rawDelta, MidpointRounding.AwayFromZero);

        return (whiteRating + roundedDelta, blackRating - roundedDelta);
    }

    private static (int NewFirstRating, int NewSecondRating) CalculateNewRatings(int firstRating, int secondRating, 
        double firstPoints)
    {
        var expectedPoints = 1.0 / (1 + Math.Pow(10, ((double)secondRating - firstRating) / 400));
        var rawDelta = KFactor * (firstPoints - expectedPoints);
        var roundedDelta = (int)Math.Round(rawDelta, MidpointRounding.AwayFromZero);

        return (firstRating + roundedDelta, secondRating - roundedDelta);
    }
}