using System.Collections.Concurrent;

namespace CoffeeChess.Core.Models;

public class GameModel(
    string gameId, 
    string firstPlayerId, 
    string firstPlayerUserName,
    GameSettingsModel gameSettings) 
{
    public string GameId { get; set; } = gameId;
    public string FirstPlayerId { get; set; } = firstPlayerId;
    public string FirstPlayerUserName { get; set; } = firstPlayerUserName;
    public string? SecondPlayerId { get; set; }
    public string? SecondPlayerUserName { get; set; }
    public GameSettingsModel GameSettings { get; set; } = gameSettings;
    public bool Started { get; set; } = false;
    public string Fen { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public ConcurrentQueue<ChatMessageModel> ChatMessages { get; } = new();
}