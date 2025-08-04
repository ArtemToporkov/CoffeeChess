using CoffeeChess.Application.Games.ReadModels;
using MediatR;

namespace CoffeeChess.Application.Games.Queries;

public record GetCompletedGameQuery(string GameId) : IRequest<CompletedGameReadModel>;