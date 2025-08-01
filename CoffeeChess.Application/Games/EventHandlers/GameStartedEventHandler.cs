﻿using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.Events;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class GameStartedEventHandler(
    IGameEventNotifierService notifier) : INotificationHandler<GameStarted>
{
    public async Task Handle(GameStarted notification, CancellationToken cancellationToken)
        => await notifier.NotifyGameStarted(
            notification.GameId, 
            notification.WhitePlayerId, 
            notification.BlackPlayerId, 
            notification.TotalMillisecondsForOnePlayerLeft, 
            cancellationToken);
}