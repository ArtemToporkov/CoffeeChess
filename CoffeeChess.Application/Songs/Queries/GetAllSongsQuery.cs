using CoffeeChess.Application.Songs.ReadModels;
using MediatR;

namespace CoffeeChess.Application.Songs.Queries;

public record GetAllSongsQuery : IRequest<IAsyncEnumerable<SongReadModel>>;