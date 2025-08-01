using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Shared.Interfaces;

namespace CoffeeChess.Domain.Chats.Repositories.Interfaces;

public interface IChatRepository : IBaseRepository<Chat>
{
    public Task SaveChangesAsync(Chat chat);
}