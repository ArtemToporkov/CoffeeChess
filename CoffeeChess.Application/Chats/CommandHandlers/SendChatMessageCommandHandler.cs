using CoffeeChess.Application.Chats.Commands;
using CoffeeChess.Application.Games.CommandHandlers;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Chats.CommandHandlers;

public class SendChatMessageCommandHandler(IChatRepository chatRepository) : IRequestHandler<SendChatMessageCommand>
{
    public async Task Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var chat = await chatRepository.GetByIdAsync(request.GameId, cancellationToken) 
                   ?? throw new NotFoundException($"Chat of game with id {request.GameId} was not found.");
        chat.AddMessage(request.Username, request.Message);
        await chatRepository.SaveChangesAsync(chat, cancellationToken);
    }
}