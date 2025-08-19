using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using MediatR;

namespace CoffeeChess.Application.Matchmaking.Commands;

public class QueueOrFindChallengeCommandHandler
    (IMatchmakingService matchmaking) : IRequestHandler<QueueOrFindChallengeCommand>
{
    public async Task Handle(QueueOrFindChallengeCommand request, CancellationToken cancellationToken)
    {
        var challengeSettings = new ChallengeSettings(
            request.Minutes, request.Increment, request.ColorPreference, request.MinRating, request.MaxRating);
        await matchmaking.QueueOrFindChallenge(request.PlayerId, challengeSettings, cancellationToken);
    }
}