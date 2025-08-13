using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Application.Songs.ReadModels;
using CoffeeChess.Application.Songs.Repositories.Interfaces;
using CoffeeChess.Application.Songs.Sevices;
using MediatR;

namespace CoffeeChess.Application.Songs.Queries;

public class GetSongCoverQueryHandler(
    ISongRepository songRepository,
    IMediaProviderService mediaProvider) : IRequestHandler<GetSongCoverQuery, FileStream>
{
    public async Task<FileStream> Handle(GetSongCoverQuery request, CancellationToken cancellationToken)
    {
        var song = await songRepository.GetByIdAsync(request.SongId, cancellationToken)
                   ?? throw new NotFoundException(nameof(SongReadModel), request.SongId);
        var stream = mediaProvider.OpenSongCoverRead(song.CoverUrl);
        return stream;
    }
}