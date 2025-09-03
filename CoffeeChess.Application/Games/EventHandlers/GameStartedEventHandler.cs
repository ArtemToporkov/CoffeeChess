using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class GameStartedEventHandler(
    IGameRepository gameRepository,
    IGameEventNotifierService notifier) : INotificationHandler<GameStarted>
{
    public async Task Handle(GameStarted notification, CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(notification.GameId, cancellationToken);
        if (game == null)
            throw new NotFoundException(nameof(Game), notification.GameId);
        await notifier.NotifyGameStarted(game.WhitePlayerId, game.BlackPlayerId, game.GameId, cancellationToken);
    }
}