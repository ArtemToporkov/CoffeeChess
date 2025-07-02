using CoffeeChess.Core.Enums;

namespace CoffeeChess.Service.Interfaces;

public interface IRatingService
{
    public (int NewFirstRating, int NewSecondRating) CalculateNewRatingsAfterDraw(
        int firstRating, int secondRating);
    
    public (int NewWinnerRating, int NewLoserRating) CalculateNewRatingsAfterWin(
        int winnerRating, int loserRating);
}