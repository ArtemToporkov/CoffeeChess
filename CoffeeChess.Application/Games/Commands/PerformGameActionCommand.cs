using CoffeeChess.Domain.Games.Enums;
using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public record PerformGameActionCommand(
    string GameId, string PlayerId, GameActionType GameActionType) : IRequest;