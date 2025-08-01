using CoffeeChess.Domain.Games.Enums;
using MediatR;

namespace CoffeeChess.Application.Commands;

public record PerformGameActionCommand(
    string GameId, string PlayerId, GameActionType GameActionType) : IRequest;