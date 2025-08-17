using CoffeeChess.Application.Chats.ReadModels;
using CoffeeChess.Application.Chats.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations;

public class SqlChatHistoryRepository(ApplicationDbContext dbContext) : IChatHistoryRepository
{
    public async Task AddAsync(ChatHistoryReadModel chatHistory, CancellationToken cancellationToken = default)
    {
        await dbContext.AddAsync(chatHistory, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ChatHistoryReadModel?> GetByIdAsync(string gameId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ChatsHistory.FirstOrDefaultAsync(chat => chat.GameId == gameId, cancellationToken);
    }
}