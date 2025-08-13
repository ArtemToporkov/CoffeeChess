using CoffeeChess.Application.Songs.ReadModels;

namespace CoffeeChess.Application.Songs.Repositories.Interfaces;

public interface ISongRepository
{
    public Task AddAsync(SongReadModel song, CancellationToken cancellationToken = default);
    public Task<SongReadModel?> GetByIdAsync(string songId, CancellationToken cancellationToken = default);
    public IAsyncEnumerable<SongReadModel> GetAllAsync(CancellationToken cancellationToken = default);
}