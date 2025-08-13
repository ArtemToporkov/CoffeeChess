using MediatR;

namespace CoffeeChess.Application.Songs.Queries;

public record GetSongCoverQuery(string SongId) : IRequest<FileStream>;