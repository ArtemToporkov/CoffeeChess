using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using MediatR;

namespace CoffeeChess.Application.Matchmaking.Commands;

public class QueueOrFindChallengeCommandHandler
    (IMatchmakingService matchmaking) : IRequestHandler<QueueOrFindChallengeCommand>
{
    public async Task Handle(QueueOrFindChallengeCommand request, CancellationToken cancellationToken)
    {
        var timeControl = new TimeControl(request.Minutes, request.Increment);
        var ratingPreference = new EloRatingPreference(request.MinRating, request.MaxRating);
        var challengeSettings = new ChallengeSettings(timeControl, request.ColorPreference, ratingPreference);
        await matchmaking.QueueOrFindChallenge(request.PlayerId, challengeSettings, cancellationToken);
    }
}