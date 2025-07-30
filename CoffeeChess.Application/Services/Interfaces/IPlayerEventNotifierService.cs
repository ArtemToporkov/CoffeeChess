namespace CoffeeChess.Application.Services.Interfaces;

public interface IPlayerEventNotifierService
{
    public Task NotifyPlayerRatingChanged(string playerId, int oldRating, int newRating);
}