using System.Collections.Concurrent;
using System.Drawing;
using ChessDotNetCore;
using CoffeeChess.Core.Enums;
using CoffeeChess.Core.Models;
using CoffeeChess.Service.Interfaces;

namespace CoffeeChess.Service.Implementations;

public class BaseGameManagerService : IGameManagerService
{
    private readonly ConcurrentDictionary<string, GameChallengeModel> _gamesChallenges = new();
    private readonly ConcurrentDictionary<string, GameModel> _games = new();
    private static readonly Random Random = new(); 

    public GameChallengeModel CreateGameChallenge(string creatorConnectionId, 
        string creatorUsername, GameSettingsModel settings)
    {
        var gameChallenge = new GameChallengeModel(creatorConnectionId, creatorConnectionId, creatorUsername, settings);
        _gamesChallenges.TryAdd(creatorConnectionId, gameChallenge);
        return gameChallenge;
    }

    public bool TryFindChallenge(string playerConnectionId, string playerUsername, GameSettingsModel settings, 
        out GameChallengeModel? foundChallenge)
    {
        foreach (var (gameChallengeId, gameChallenge) in _gamesChallenges)
        {
            if (gameChallenge.PlayerId != playerConnectionId)
            {
                _gamesChallenges.TryRemove(gameChallengeId, out foundChallenge);
                return true;
            }
        }

        foundChallenge = null;
        return false;
    }

    public GameModel CreateGameBasedOnFoundChallenge(string playerConnectionId, 
        GameSettingsModel settings, GameChallengeModel gameChallenge)
    {
        var connectionPlayerColor = GetColor(settings);
        var (whitePlayerId, blackPlayerId) = connectionPlayerColor == ColorPreference.White
            ? (playerConnectionId, gameChallenge.PlayerId)
            : (gameChallenge.PlayerId, playerConnectionId);
        var createdGame = new GameModel
        {
            GameId = Guid.NewGuid().ToString("N")[..8],
            WhitePlayerId = whitePlayerId,
            BlackPlayerId = blackPlayerId,
            WhiteTimeLeft = TimeSpan.FromMinutes(settings.Minutes),
            BlackTimeLeft = TimeSpan.FromMinutes(settings.Minutes),
            Increment = TimeSpan.FromSeconds(settings.Increment)
        };
        _games.TryAdd(createdGame.GameId, createdGame);
        return createdGame;
    }
    
    public bool TryAddChatMessage(string gameId, string username, string message)
    {
        if (!_games.TryGetValue(gameId, out var game)) 
            return false;
        game.ChatMessages.Enqueue(new ChatMessageModel { Username = username, Message = message });
        return true;
    }

    public bool TryGetGame(string gameId, out GameModel? game)
    {
        if (_games.TryGetValue(gameId, out game))
            return true;

        game = null;
        return false;
    }
    
    private static ColorPreference GetColor(GameSettingsModel settings)
        => settings.ColorPreference switch
        {
            ColorPreference.White => ColorPreference.White,
            ColorPreference.Black => ColorPreference.Black,
            ColorPreference.Any => Random.Next(0, 2) == 0
                ? ColorPreference.White
                : ColorPreference.Black,
            _ => throw new ArgumentException("[BaseGameManageService.GetColor]: Unsupported color preference")
        };
}
