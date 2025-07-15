using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using CoffeeChess.Application.Interfaces;
using CoffeeChess.Domain.Aggregates;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Application.Services;

public class BaseGameManagerService : IGameManagerService
{
    private readonly ConcurrentDictionary<string, GameChallenge> _gamesChallenges = new();
    private readonly ConcurrentDictionary<string, Game> _games = new();
    private static readonly Random Random = new();
    private static readonly Lock Lock = new();

    public Game? CreateGameOrQueueChallenge(PlayerInfo playerInfo, GameSettings settings)
    {
        lock (Lock)
        {
            if (TryFindChallenge(playerInfo, out var foundChallenge))
                return CreateGameBasedOnFoundChallenge(playerInfo, settings, foundChallenge);
            CreateGameChallenge(playerInfo, settings);
            return null;
        }
    }
    
    public bool TryAddChatMessage(string gameId, string username, string message)
    {
        if (!_games.TryGetValue(gameId, out var game)) 
            return false;
        game.ChatMessages.Enqueue(new ChatMessage { Username = username, Message = message });
        return true;
    }

    public bool TryGetGame(string gameId, [NotNullWhen(true)] out Game? game)
    {
        if (_games.TryGetValue(gameId, out game))
            return true;

        game = null;
        return false;
    }

    public IEnumerable<Game> GetActiveGames() => _games.Values
        .Where(g => !g.IsOver);
    
    private Game CreateGameBasedOnFoundChallenge(PlayerInfo connectingPlayerInfo, 
        GameSettings settings, GameChallenge gameChallenge)
    {
        var connectingPlayerColor = GetColor(settings);
        var (whitePlayerInfo, blackPlayerInfo) = connectingPlayerColor == ColorPreference.White
            ? (connectingPlayerInfo, gameChallenge.PlayerInfo)
            : (gameChallenge.PlayerInfo, connectingPlayerInfo);
        var createdGame = new Game(
            Guid.NewGuid().ToString("N")[..8],
            whitePlayerInfo,
            blackPlayerInfo,
            TimeSpan.FromMinutes(settings.Minutes),
            TimeSpan.FromSeconds(settings.Increment)
        );
        _games.TryAdd(createdGame.GameId, createdGame);
        return createdGame;
    }
    
    private void CreateGameChallenge(PlayerInfo creatorInfo, GameSettings settings)
    {
        var gameChallenge = new GameChallenge(creatorInfo, settings);
        _gamesChallenges.TryAdd(creatorInfo.Id, gameChallenge);
    }

    private bool TryFindChallenge(PlayerInfo playerInfo, 
        [NotNullWhen(true)] out GameChallenge? foundChallenge)
    {
        foreach (var (gameChallengeId, gameChallenge) in _gamesChallenges)
        {
            if (gameChallenge.PlayerInfo.Id != playerInfo.Id)
            {
                _gamesChallenges.TryRemove(gameChallengeId, out foundChallenge);
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
