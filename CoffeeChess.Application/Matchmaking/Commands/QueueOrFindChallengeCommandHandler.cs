using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using MediatR;

namespace CoffeeChess.Application.Matchmaking.Commands;

public class QueueOrFindChallengeCommandHandler
    (IMatchmakingService matchmaking) : IRequestHandler<QueueOrFindChallengeCommand>
{
    public Task Handle(QueueOrFindChallengeCommand request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}