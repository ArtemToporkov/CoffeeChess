using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Repositories.Interfaces;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Services;

public class BaseGameManagerService(
    IChallengeRepository challengeRepository, 
    IGameRepository gameRepository) : IGameManagerService
{
    private static readonly Random Random = new();
    private static readonly Lock Lock = new();

    public Game? CreateGameOrQueueChallenge(string playerId, GameSettings settings)
    {
        lock (Lock)
        {
            if (TryFindChallenge(playerId, out var foundChallenge))
                return CreateGameBasedOnFoundChallenge(playerId, settings, foundChallenge);
            CreateGameChallenge(playerId, settings);
            return null;
        }
    }
    
    public bool TryAddChatMessage(string gameId, string username, string message)
    {
        if (!gameRepository.TryGetValue(gameId, out var game)) 
            return false;
        game.Chat.AddMessage(username, message);
        return true;
    }
    
    private Game CreateGameBasedOnFoundChallenge(string connectingPlayerId, 
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
        return createdGame;
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
            _ => throw new ArgumentException($"[{nameof(BaseGameManagerService)}.{nameof(ChooseColor)}]: " +
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
