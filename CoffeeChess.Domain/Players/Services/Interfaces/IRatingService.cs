using CoffeeChess.Domain.Games.Enums;

namespace CoffeeChess.Domain.Players.Services.Interfaces;

public interface IRatingService
{
    public (int NewWhiteRating, int NewBlackRating) CalculateNewRatings(
        int whiteRating, int blackRating, GameResult gameResult);
}