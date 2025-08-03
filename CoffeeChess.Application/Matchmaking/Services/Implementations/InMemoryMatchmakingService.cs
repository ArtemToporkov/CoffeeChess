using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Entities;

namespace CoffeeChess.Application.Matchmaking.Services.Implementations;

public class InMemoryMatchmakingService(
    IChallengeRepository challengeRepository, 
    IGameRepository gameRepository,
    IChatRepository chatRepository) : IMatchmakingService
{
    private static readonly Random Random = new();
    private static readonly Lock Lock = new();
    private static readonly SemaphoreSlim Mutex = new(1, 1);

    public async Task QueueChallenge(
        string playerId, GameSettings settings, CancellationToken cancellationToken = default)
    {
        await Mutex.WaitAsync(cancellationToken);
        try
        {
            if (TryFindChallenge(playerId, out var foundChallenge))
            {
                await CreateGameBasedOnFoundChallenge(playerId, settings, foundChallenge, cancellationToken);
                return;
            }

            await CreateGameChallenge(playerId, settings, cancellationToken);
        }
        finally
        {
            Mutex.Release();
        }
    }
    
    private async Task CreateGameBasedOnFoundChallenge(string connectingPlayerId,
        GameSettings settings, GameChallenge gameChallenge, CancellationToken cancellationToken = default)
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
        await gameRepository.AddAsync(createdGame, cancellationToken);
        var chat = new Chat(createdGame.GameId);
        await chatRepository.AddAsync(chat, cancellationToken);
        await gameRepository.SaveChangesAsync(createdGame, cancellationToken);
    }
    
    private async Task CreateGameChallenge(string creatorId, GameSettings settings, 
        CancellationToken cancellationToken = default)
    {
        var gameChallenge = new GameChallenge(creatorId, settings);
        await challengeRepository.AddAsync(gameChallenge, cancellationToken);
    }

    private bool TryFindChallenge(string playerId, 
        [NotNullWhen(true)] out GameChallenge? foundChallenge)
    {
        // TODO: find appropriate challenge
        foreach (var gameChallenge in challengeRepository.GetAll())
        {
            if (gameChallenge.PlayerId != playerId)
            {
                // TODO: fix async problems
                challengeRepository.DeleteAsync(gameChallenge);
                foundChallenge = gameChallenge;
                return true;
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
