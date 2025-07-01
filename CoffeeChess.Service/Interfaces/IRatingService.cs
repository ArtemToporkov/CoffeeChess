using CoffeeChess.Core.Enums;

namespace CoffeeChess.Service.Interfaces;

public interface IRatingService
{
    public (int NewWhiteRating, int NewBlackRating) CalculateNewRatings(
        int whiteRating, int blackRating, Result result);
}