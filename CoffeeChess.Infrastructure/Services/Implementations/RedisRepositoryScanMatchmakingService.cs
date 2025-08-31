using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;

namespace CoffeeChess.Infrastructure.Services.Implementations;

public class RedisRepositoryScanMatchmakingService(
    IChallengeRepository challengeRepository) : IMatchmakingService
{
    private static readonly SemaphoreSlim Mutex = new(1, 1);

    public async Task<bool> QueueOrFindMatchingChallenge(
        string playerId, int playerRating, ChallengeSettings settings, CancellationToken cancellationToken = default)
    {
        await Mutex.WaitAsync(cancellationToken);
        try
        {
            var challenge = await TryFindChallenge(playerId, playerRating, settings, cancellationToken);
            return challenge is not null;
        }
        finally
        {
            Mutex.Release();
        }
    }

    public Task AddAsync(Challenge challenge)
    {
        // NOTE: created for benchmark purposes,
        // this service implementation uses IChallengeRepository.AddAsync()
        return Task.CompletedTask;
    }

    private async Task <Challenge?> TryFindChallenge(
        string playerId, int playerRating, ChallengeSettings settings, CancellationToken cancellationToken = default)
    {
        foreach (var gameChallenge in challengeRepository.GetAll()
                     .Where(c => 
                         c.PlayerId != playerId && ValidatePlayerForChallenge(playerRating, settings, c)))
        {
            await challengeRepository.DeleteAsync(gameChallenge, cancellationToken);
            return gameChallenge;
        }

        return null;
    }

    private static bool ValidatePlayerForChallenge(
        int playerRating, ChallengeSettings playerSettings, Challenge challengeToJoin)
    {
        return playerSettings.TimeControl.Minutes == challengeToJoin.ChallengeSettings.TimeControl.Minutes
               && playerSettings.TimeControl.Increment == challengeToJoin.ChallengeSettings.TimeControl.Increment
               && playerRating >= challengeToJoin.ChallengeSettings.EloRatingPreference.Min
               && playerRating <= challengeToJoin.ChallengeSettings.EloRatingPreference.Max
               && challengeToJoin.PlayerRating >= playerSettings.EloRatingPreference.Min
               && challengeToJoin.PlayerRating <= playerSettings.EloRatingPreference.Max
               && (playerSettings.ColorPreference == ColorPreference.Any
                   || challengeToJoin.ChallengeSettings.ColorPreference == ColorPreference.Any 
                   || playerSettings.ColorPreference == ColorPreference.White 
                       && challengeToJoin.ChallengeSettings.ColorPreference == ColorPreference.Black
                   || playerSettings.ColorPreference == ColorPreference.Black
                       && challengeToJoin.ChallengeSettings.ColorPreference == ColorPreference.White);
    }
}
