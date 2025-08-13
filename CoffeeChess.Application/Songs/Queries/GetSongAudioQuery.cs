using MediatR;

namespace CoffeeChess.Application.Songs.Queries;

public record GetSongAudioQuery(string SongId) : IRequest<FileStream>;