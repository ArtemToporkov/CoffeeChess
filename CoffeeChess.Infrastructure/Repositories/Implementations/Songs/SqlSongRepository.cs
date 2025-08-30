using CoffeeChess.Application.Songs.ReadModels;
using CoffeeChess.Application.Songs.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoffeeChess.Infrastructure.Repositories.Implementations.Songs;

public class SqlSongRepository(ApplicationDbContext dbContext) : ISongRepository
{
    public async Task AddAsync(SongReadModel song, CancellationToken cancellationToken = default)
    {
        await dbContext.Songs.AddAsync(song, cancellationToken);
    }

    public async Task<SongReadModel?> GetByIdAsync(string songId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Songs.FirstOrDefaultAsync(s => s.SongId == songId, cancellationToken);
    }

    public IAsyncEnumerable<SongReadModel> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Songs.AsAsyncEnumerable();
    }
}