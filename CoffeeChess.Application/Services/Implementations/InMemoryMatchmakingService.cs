using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Application.Services.Interfaces;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Services.Implementations;

public class InMemoryMatchmakingService(
    IChallengeRepository challengeRepository, 
    IGameRepository gameRepository,
    IChatRepository chatRepository) : IMatchmakingService
{
    private static readonly Random Random = new();
    private static readonly Lock Lock = new();
    private static readonly SemaphoreSlim Mutex = new(1, 1);

    public async Task QueueChallenge(string playerId, GameSettings settings)
    {
        await Mutex.WaitAsync();
        try
        {
            if (TryFindChallenge(playerId, out var foundChallenge))
            {
                await CreateGameBasedOnFoundChallenge(playerId, settings, foundChallenge);
                return;
            }

            CreateGameChallenge(playerId, settings);
        }
        finally
        {
            Mutex.Release();
        }
    }
    
    private async Task CreateGameBasedOnFoundChallenge(string connectingPlayerId,
        GameSettings settings, GameChallenge gameChallenge)
    {
        var connectingPlayerColor = ChooseColor(settings);
        var (whitePlayerId, blackPlayerId) = connectingPlayerColor == ColorPreference.White
            ? (connectingPlayerId, gameChallenge.PlayerId)
            : (gameChallenge.PlayerId, connectingPlayerId);
        var createdGame = new Game(
            Guid.NewGuid().ToString("N")[..8],
            whitePlayerId,
            blackPlayerId,
            TimeSpan.FromMinutes(settings.Minutes),
            TimeSpan.FromSeconds(settings.Increment)
        );
        gameRepository.TryAdd(createdGame.GameId, createdGame);
        var chat = new Chat(createdGame.GameId);
        chatRepository.TryAdd(chat.GameId, chat);
        await gameRepository.SaveChangesAsync(createdGame);
    }
    
    private void CreateGameChallenge(string creatorId, GameSettings settings)
    {
        var gameChallenge = new GameChallenge(creatorId, settings);
        challengeRepository.TryAdd(creatorId, gameChallenge);
    }

    private bool TryFindChallenge(string playerId, 
        [NotNullWhen(true)] out GameChallenge? foundChallenge)
    {
        foreach (var (gameChallengeId, gameChallenge) in challengeRepository.GetAll())
        {
            if (gameChallenge.PlayerId != playerId)
            {
                challengeRepository.TryRemove(gameChallengeId, out foundChallenge);
                return foundChallenge is not null;
            }
        }

        foundChallenge = null;
        return false;
    }

    private static ColorPreference ChooseColor(GameSettings settings)
        => settings.ColorPreference switch
        {
            ColorPreference.White => ColorPreference.White,
            ColorPreference.Black => ColorPreference.Black,
            ColorPreference.Any => GetRandomColor(),
            _ => throw new ArgumentException($"[{nameof(InMemoryMatchmakingService)}.{nameof(ChooseColor)}]: " +
                                             $"Unsupported color preference.")
        };

    private static ColorPreference GetRandomColor()
    {
        lock (Lock)
            return Random.Next(0, 2) == 0
                ? ColorPreference.White
                : ColorPreference.Black;
    }
}
