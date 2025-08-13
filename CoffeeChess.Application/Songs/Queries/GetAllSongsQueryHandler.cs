using CoffeeChess.Application.Songs.ReadModels;
using CoffeeChess.Application.Songs.Repositories.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Songs.Queries;

public class GetAllSongsQueryHandler(
    ISongRepository songRepository) : IRequestHandler<GetAllSongsQuery, IAsyncEnumerable<SongReadModel>>
{
    public Task<IAsyncEnumerable<SongReadModel>> Handle(GetAllSongsQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(songRepository.GetAllAsync(cancellationToken));
    }
}