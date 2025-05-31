using System.Collections.Concurrent;
using CoffeeChess.Core.Models;
using CoffeeChess.Service.Interfaces;

namespace CoffeeChess.Service.Implementations;

public class BaseGameManagerService : IGameManagerService
{
    private readonly ConcurrentDictionary<string, GameModel> _games = new();
    private readonly ConcurrentQueue<string> _pendingGameIds = new();

    public GameModel CreateGame(string creatorConnectionId, string creatorUsername, GameSettingsModel settings)
    {
        var gameId = Guid.NewGuid().ToString("N")[..8];
        var gameState = new GameModel(gameId, creatorConnectionId, creatorUsername, settings);
        _games.TryAdd(gameId, gameState);
        _pendingGameIds.Enqueue(gameId);
        return gameState;
    }

    public bool TryJoinGame(string gameId, string joinerConnectionId, string joinerUsername, out GameModel? joinedGame)
    {
        if (_games.TryGetValue(gameId, out var game) && string.IsNullOrEmpty(game.SecondPlayerId))
        {
            game.SecondPlayerId = joinerConnectionId;
            game.SecondPlayerUserName = joinerUsername;
            game.Started = true;
            joinedGame = game;
            return true;
        }

        joinedGame = null;
        return false;
    }

    public GameModel FindOrCreateGame(string playerConnectionId, string playerUsername, GameSettingsModel settings)
    {
        while (_pendingGameIds.TryDequeue(out var gameIdToJoin))
        {
            if (_games.TryGetValue(gameIdToJoin, out var gameToJoin) && string.IsNullOrEmpty(gameToJoin.SecondPlayerId))
            {
                if (gameToJoin.FirstPlayerId != playerConnectionId)
                {
                    gameToJoin.SecondPlayerId = playerConnectionId;
                    gameToJoin.SecondPlayerUserName = playerUsername;
                    gameToJoin.Started = true;
                    return gameToJoin;
                }

                _pendingGameIds.Enqueue(gameIdToJoin);
            }
        }

        return CreateGame(playerConnectionId, playerUsername, settings);
    }


    public bool TryGetGame(string gameId, out GameModel? game)
    {
        if (_games.TryGetValue(gameId, out var foundGame))
        {
            game = foundGame;
            return true;
        }

        game = null;
        return false;
    }

    public bool TryAddChatMessage(string gameId, string username, string message)
    {
        if (!TryGetGame(gameId, out var game)) 
            return false;
        game!.ChatMessages.Enqueue(new ChatMessageModel { Username = username, Message = message });
        return true;
    }
}