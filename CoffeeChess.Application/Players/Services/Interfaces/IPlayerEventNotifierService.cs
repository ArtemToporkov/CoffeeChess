namespace CoffeeChess.Application.Players.Services.Interfaces;

public interface IPlayerEventNotifierService
{
    public Task NotifyPlayerRatingChanged(string playerId, int oldRating, int newRating);
}