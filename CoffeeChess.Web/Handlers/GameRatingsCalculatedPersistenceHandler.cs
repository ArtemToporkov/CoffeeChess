using CoffeeChess.Web.Models;
using CoffeeChess.Web.Notifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeeChess.Web.Handlers;

public class GameRatingsCalculatedPersistenceHandler(UserManager<UserModel> userManager) : INotificationHandler<GameResultCalculatedNotification>
{
    public async Task Handle(GameResultCalculatedNotification notification, CancellationToken cancellationToken)
    {
        await UpdateRating(notification.WhitePlayerInfo.Id, notification.GameResultPayloadForWhite.NewRating);
        await UpdateRating(notification.BlackPlayerInfo.Id, notification.GameResultPayloadForBlack.NewRating);
    }

    private async Task UpdateRating(string userId, int? newRating)
    {
        if (newRating is null)
            return;
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return;
        user.Rating = newRating.Value;
        await userManager.UpdateAsync(user);
    }
}