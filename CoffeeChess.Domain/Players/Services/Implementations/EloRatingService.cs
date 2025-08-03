using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Players.Services.Interfaces;

namespace CoffeeChess.Domain.Players.Services.Implementations;

public class EloRatingService : IRatingService
{
    private const int KFactor = 15;

    public (int NewWhiteRating, int NewBlackRating) CalculateNewRatings(int whiteRating, int blackRating,
        GameResult gameResult)
    {
        var expectedWhitePoints = 1.0 / (1 + Math.Pow(10, ((double)blackRating - whiteRating) / 400));
        var actualWhitePoints = gameResult switch
        {
            GameResult.WhiteWon => 1.0,
            GameResult.Draw => 0.5,
            GameResult.BlackWon => 0.0,
            _ => throw new ArgumentOutOfRangeException(nameof(gameResult), gameResult, null)
        };
        
        var rawDelta = KFactor * (actualWhitePoints - expectedWhitePoints);
        var roundedDelta = (int)Math.Round(rawDelta, MidpointRounding.AwayFromZero);

        return (whiteRating + roundedDelta, blackRating - roundedDelta);
    }
}