﻿using CoffeeChess.Web.Models;
using CoffeeChess.Web.Notifications;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CoffeeChess.Web.Handlers;

public class UpdatePlayerRatingsHandler(UserManager<UserModel> userManager) : INotificationHandler<GameResultCalculatedNotification>
{
    public async Task Handle(GameResultCalculatedNotification notification, CancellationToken cancellationToken)
    {
        await UpdateRating(notification.FirstPlayer.Id, notification.GameResultPayloadForFirst.NewRating);
        await UpdateRating(notification.SecondPlayer.Id, notification.GameResultPayloadForSecond.NewRating);
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