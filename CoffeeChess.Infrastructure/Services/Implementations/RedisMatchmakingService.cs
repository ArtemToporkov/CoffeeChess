using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;

namespace CoffeeChess.Infrastructure.Services.Implementations;

public class RedisMatchmakingService(
    IPlayerRepository playerRepository,
    IChallengeRepository challengeRepository, 
    IGameRepository gameRepository,
    IChatRepository chatRepository) : IMatchmakingService
{
    private static readonly Random Random = new();
    private static readonly Lock Lock = new();
    private static readonly SemaphoreSlim Mutex = new(1, 1);

    public async Task QueueOrFindChallenge(
        string playerId, ChallengeSettings settings, CancellationToken cancellationToken = default)
    {
        var playerRating = (await playerRepository.GetByIdAsync(playerId, cancellationToken))?.Rating
            ?? throw new NotFoundException(nameof(Player), playerId);
        
        await Mutex.WaitAsync(cancellationToken);
        try
        {
            var challenge = await TryFindChallenge(playerId, playerRating, settings, cancellationToken);
            if (challenge is not null)
            {
                await CreateGameBasedOnFoundChallenge(playerId, settings, challenge, cancellationToken);
                return;
            }

            await CreateGameChallenge(playerId, playerRating, settings, cancellationToken);
        }
        finally
        {
            Mutex.Release();
        }
    }
    
    private async Task CreateGameBasedOnFoundChallenge(string connectingPlayerId,
        ChallengeSettings settings, Challenge challenge, CancellationToken cancellationToken = default)
    {
        var connectingPlayerColor = ChooseColor(settings);
        var (whitePlayerId, blackPlayerId) = connectingPlayerColor == ColorPreference.White
            ? (connectingPlayerId, challenge.PlayerId)
            : (challenge.PlayerId, connectingPlayerId);
        var createdGame = new Game(
            Guid.NewGuid().ToString("N")[..8],
            whitePlayerId,
            blackPlayerId,
            TimeSpan.FromMinutes(settings.TimeControl.Minutes),
            TimeSpan.FromSeconds(settings.TimeControl.Increment)
        );
        await gameRepository.AddAsync(createdGame, cancellationToken);
        var chat = new Chat(createdGame.GameId);
        await chatRepository.AddAsync(chat, cancellationToken);
        await gameRepository.SaveChangesAsync(createdGame, cancellationToken);
    }
    
    private async Task CreateGameChallenge(string creatorId, int creatorRating, ChallengeSettings settings, 
        CancellationToken cancellationToken = default)
    {
        var gameChallenge = new Challenge(creatorId, creatorRating, settings);
        await challengeRepository.AddAsync(gameChallenge, cancellationToken);
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

    private static ColorPreference ChooseColor(ChallengeSettings settings)
        => settings.ColorPreference switch
        {
            ColorPreference.White => ColorPreference.White,
            ColorPreference.Black => ColorPreference.Black,
            ColorPreference.Any => GetRandomColor(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(settings.ColorPreference), settings.ColorPreference, "Unexpected color preference.")
        };

    private static ColorPreference GetRandomColor()
    {
        lock (Lock)
            return Random.Next(0, 2) == 0
                ? ColorPreference.White
                : ColorPreference.Black;
    }
}
