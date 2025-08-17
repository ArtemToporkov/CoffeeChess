using CoffeeChess.Application.Chats.ReadModels;
using CoffeeChess.Application.Chats.Repositories.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Events;
using MediatR;

namespace CoffeeChess.Application.Chats.EventHandlers;

public class GameEndedEventHandler(
    IChatRepository chatRepository, 
    IChatHistoryRepository chatHistoryRepository) : INotificationHandler<GameEnded>
{
    public async Task Handle(GameEnded notification, CancellationToken cancellationToken)
    {
        var chat = await chatRepository.GetByIdAsync(notification.GameId, cancellationToken)
            ?? throw new NotFoundException(
                $"{nameof(Chat)} for {nameof(Game)} with ID \"{notification.GameId}\" was not found.");
        var chatHistoryReadModel = new ChatHistoryReadModel
        {
            GameId = chat.GameId,
            Messages = chat.Messages.ToList()
        };
        await chatHistoryRepository.AddAsync(chatHistoryReadModel, cancellationToken);
    }
}