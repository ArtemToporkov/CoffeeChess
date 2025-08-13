using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Application.Songs.ReadModels;
using CoffeeChess.Application.Songs.Repositories.Interfaces;
using CoffeeChess.Application.Songs.Sevices;
using MediatR;

namespace CoffeeChess.Application.Songs.Queries;

public class GetSongAudioQueryHandler(
    ISongRepository songRepository, 
    IMediaProviderService mediaProvider) : IRequestHandler<GetSongAudioQuery, FileStream>
{
    public async Task<FileStream> Handle(GetSongAudioQuery request, CancellationToken cancellationToken)
    {
        var song = await songRepository.GetByIdAsync(request.SongId, cancellationToken)
            ?? throw new NotFoundException(nameof(SongReadModel), request.SongId);
        var stream = mediaProvider.OpenSongAudioRead(song.AudioUrl);
        return stream;
    }
}