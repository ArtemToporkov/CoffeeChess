using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Services.Interfaces;

public interface IRatingService
{
    public (int NewFirstRating, int NewSecondRating) CalculateNewRatingsAfterDraw(
        int firstRating, int secondRating);
    
    public (int NewWinnerRating, int NewLoserRating) CalculateNewRatingsAfterWin(
        int winnerRating, int loserRating);

    public (int NewWhiteRating, int NewBlackRating) CalculateNewRatings(
        int whiteRating, int blackRating, Result result);
}