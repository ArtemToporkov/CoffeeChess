using CoffeeChess.Core;

public class GameState(Guid gameId)
{
    public Guid GameId { get; } = gameId;
    public string? PlayerWhiteId { get; set; }
    public string? PlayerBlackId { get; set; }
    public string CurrentFen { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public List<MoveRecord> Moves { get; set; } = [];
    public TimeSpan PlayerWhiteTimeLeft { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan PlayerBlackTimeLeft { get; set; } = TimeSpan.FromMinutes(5);
    public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;
    public string? CurrentTurnPlayerId { get; set; }
    public bool IsGameOver { get; set; } = false;
    public CancellationTokenSource? TimerTokenSource { get; set; }


    public bool IsWhiteTurn => CurrentTurnPlayerId == PlayerWhiteId;

    public void SwitchTurn() 
        => CurrentTurnPlayerId = CurrentTurnPlayerId == PlayerWhiteId 
            ? PlayerBlackId 
            : PlayerWhiteId;

    public void StopTimer()
    {
        TimerTokenSource?.Cancel();
        TimerTokenSource = null;
    }
}