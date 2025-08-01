using CoffeeChess.Application.Chats.Commands;
using CoffeeChess.Application.Games.CommandHandlers;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Chats.CommandHandlers;

public class SendChatMessageCommandHandler(IChatRepository chatRepository) : IRequestHandler<SendChatMessageCommand>
{
    public async Task Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (!chatRepository.TryGetValue(request.GameId, out var chat))
            throw new InvalidOperationException(
                $"[{nameof(PerformGameActionCommandHandler)}.{nameof(Handle)}]: chat not found.");
        await chat.AddMessage(request.Username, request.Message);
        await chatRepository.SaveChangesAsync(chat);
    }
}