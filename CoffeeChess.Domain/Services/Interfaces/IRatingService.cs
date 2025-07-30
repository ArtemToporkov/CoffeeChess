using CoffeeChess.Domain.Enums;

namespace CoffeeChess.Domain.Services.Interfaces;

public interface IRatingService
{
    public (int NewWhiteRating, int NewBlackRating) CalculateNewRatings(
        int whiteRating, int blackRating, GameResult gameResult);
}