using CoffeeChess.Application.Games.Dto;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public record CheckForActiveGamesCommand(string PlayerId) : IRequest<PlayerGameInfoDto?>;