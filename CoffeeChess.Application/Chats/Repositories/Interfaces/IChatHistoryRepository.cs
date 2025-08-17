using CoffeeChess.Application.Chats.ReadModels;

namespace CoffeeChess.Application.Chats.Repositories.Interfaces;

public interface IChatHistoryRepository
{
    public Task AddAsync(ChatHistoryReadModel chatHistory, CancellationToken cancellationToken = default);
    
    public Task<ChatHistoryReadModel?> GetByIdAsync(string gameId, CancellationToken cancellationToken = default);
}