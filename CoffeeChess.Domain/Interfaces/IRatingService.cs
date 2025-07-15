namespace CoffeeChess.Domain.Interfaces;

public interface IRatingService
{
    public (int NewFirstRating, int NewSecondRating) CalculateNewRatingsAfterDraw(
        int firstRating, int secondRating);
    
    public (int NewWinnerRating, int NewLoserRating) CalculateNewRatingsAfterWin(
        int winnerRating, int loserRating);
}