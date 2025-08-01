﻿using MediatR;

namespace CoffeeChess.Application.Games.Commands;

public record MakeMoveCommand(
    string GameId, string PlayerId, string From, string To, string? Promotion) : IRequest;