using CoffeeChess.Application.Chats.ReadModels;
using CoffeeChess.Application.Chats.Repositories.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Games.AggregatesRoots;
using MediatR;

namespace CoffeeChess.Application.Chats.Queries;

public class GetChatHistoryQueryHandler(
    IChatHistoryRepository chatHistoryRepository) : IRequestHandler<GetChatHistoryQuery, ChatHistoryReadModel>
{
    public async Task<ChatHistoryReadModel> Handle(GetChatHistoryQuery request, CancellationToken cancellationToken)
    {
        // TODO: decide if someone other than the game players can see the chat
        return await chatHistoryRepository.GetByIdAsync(request.GameId, cancellationToken) 
               ?? throw new NotFoundException(
                   $"{nameof(Chat)} for {nameof(Game)} with ID \"{request.GameId}\" was not found.");
    }
}