using CoffeeChess.Application.Chats.Services.Interfaces;
using CoffeeChess.Domain.Chats.Events;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Chats.EventHandlers;

public class ChatMessageAddedEventHandler(
    IGameRepository gameRepository,
    IChatEventNotifierService notifier) : INotificationHandler<ChatMessageAdded>
{
    public async Task Handle(ChatMessageAdded notification, CancellationToken cancellationToken)
    {
        if (!gameRepository.TryGetValue(notification.GameId, out var game))
            throw new InvalidOperationException(
                $"[{nameof(ChatMessageAddedEventHandler)}.{nameof(Handle)}]: game not found.");
        await notifier.NotifyChatMessageAdded(game.WhitePlayerId, game.BlackPlayerId, notification.Username,
            notification.Message);
    }
}