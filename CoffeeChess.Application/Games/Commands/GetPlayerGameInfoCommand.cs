using CoffeeChess.Application.Games.Dto;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public record GetPlayerGameInfoCommand(string GameId, string PlayerId) : IRequest<PlayerGameInfoDto?>;