using System.Collections.Concurrent;
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

    public Game? CreateGameOrQueueChallenge(Player player, GameSettings settings)
    {
        lock (Lock)
        {
            if (TryFindChallenge(player, out var foundChallenge))
                return CreateGameBasedOnFoundChallenge(player, settings, foundChallenge);
            CreateGameChallenge(player, settings);
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
    
    private Game CreateGameBasedOnFoundChallenge(Player connectingPlayer, 
        GameSettings settings, GameChallenge gameChallenge)
    {
        var connectingPlayerColor = GetColor(settings);
        var (whitePlayerInfo, blackPlayerInfo) = connectingPlayerColor == ColorPreference.White
            ? (connectingPlayerInfo: connectingPlayer, PlayerInfo: gameChallenge.Player)
            : (PlayerInfo: gameChallenge.Player, connectingPlayerInfo: connectingPlayer);
        var createdGame = new Game(
            Guid.NewGuid().ToString("N")[..8],
            whitePlayerInfo,
            blackPlayerInfo,
            TimeSpan.FromMinutes(settings.Minutes),
            TimeSpan.FromSeconds(settings.Increment)
        );
        gameRepository.TryAdd(createdGame.GameId, createdGame);
        return createdGame;
    }
    
    private void CreateGameChallenge(Player creator, GameSettings settings)
    {
        var gameChallenge = new GameChallenge(creator, settings);
        challengeRepository.TryAdd(creator.Id, gameChallenge);
    }

    private bool TryFindChallenge(Player player, 
        [NotNullWhen(true)] out GameChallenge? foundChallenge)
    {
        foreach (var (gameChallengeId, gameChallenge) in challengeRepository.GetAll())
        {
            if (gameChallenge.Player.Id != player.Id)
            {
                challengeRepository.TryRemove(gameChallengeId, out foundChallenge);
                return foundChallenge is not null;
            }
        }

        foundChallenge = null;
        return false;
    }

    private static ColorPreference GetColor(GameSettings settings)
        => settings.ColorPreference switch
        {
            ColorPreference.White => ColorPreference.White,
            ColorPreference.Black => ColorPreference.Black,
            ColorPreference.Any => GetRandomColor(),
            _ => throw new ArgumentException($"[{nameof(BaseGameManagerService)}.{nameof(GetColor)}]: " +
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
