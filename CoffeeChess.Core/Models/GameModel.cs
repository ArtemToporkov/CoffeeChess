using System.Collections.Concurrent;
using System.Text;
using ChessDotNetCore;
using CoffeeChess.Core.Enums;

namespace CoffeeChess.Core.Models;

public class GameModel(
    string gameId, 
    PlayerInfoModel whitePlayerInfo,
    PlayerInfoModel blackPlayerInfo, 
    TimeSpan minutesLeftForPlayer,
    TimeSpan increment)
{
    public string GameId { get; init; } = gameId;
    public PlayerInfoModel WhitePlayerInfo { get; init; } = whitePlayerInfo;
    public PlayerInfoModel BlackPlayerInfo { get; init; } = blackPlayerInfo;
    public bool IsOver => _chessGame.GameResult != GameResult.OnGoing;
    public TimeSpan WhiteTimeLeft { get; private set; } = minutesLeftForPlayer;
    public TimeSpan BlackTimeLeft { get; private set; } = minutesLeftForPlayer;
    public TimeSpan Increment { get; init; } = increment;
    public DateTime LastMoveTime { get; set; } = DateTime.UtcNow;
    public DateTime TimeExpiresAt { get; private set; } = DateTime.UtcNow + minutesLeftForPlayer;
    public ConcurrentQueue<ChatMessageModel> ChatMessages { get; } = new();
    public bool IsWhiteTurn => _chessGame.CurrentPlayer == Player.White;
    private readonly ChessGame _chessGame = new();
    private readonly Lock _lockObject = new();

    public MoveResult MakeMove(string playerId, string from, string to, string? promotion)
    {
        var currentPlayerId = IsWhiteTurn ? WhitePlayerInfo.Id : BlackPlayerInfo.Id;
        
        if (playerId != currentPlayerId)
            return MoveResult.NotYourTurn;
        
        ReduceTime(IsWhiteTurn);
        if (DateTime.UtcNow >= TimeExpiresAt)
        {
            LoseOnTimeOrThrow();
            return MoveResult.TimeRanOut;
        }

        var promotionChar = promotion?[0];

        var move = new Move(from, to, IsWhiteTurn ? Player.White : Player.Black, promotionChar);
        if (_chessGame.MakeMove(move, false) is MoveType.Invalid)
            return MoveResult.Invalid;

        if (_chessGame.ThreeFoldRepeatAndThisCanResultInDraw)
            return MoveResult.ThreeFold;

        if (_chessGame.FiftyMovesAndThisCanResultInDraw)
            return MoveResult.FiftyMovesRule;
        
        if (_chessGame.IsCheckmated(Player.White) || _chessGame.IsCheckmated(Player.Black))
            return MoveResult.Checkmate;
        
        if (_chessGame.IsStalemated(Player.White) || _chessGame.IsStalemated(Player.Black))
            return MoveResult.Stalemate;
        
        DoIncrement(IsWhiteTurn);
        TimeExpiresAt = DateTime.UtcNow + (IsWhiteTurn ? WhiteTimeLeft : BlackTimeLeft);
        return MoveResult.Success;
    }

    public string GetPgn()
    {
        var pgnBuilder = new StringBuilder();
        for (var i = 0; i < _chessGame.Moves.Count; i++)
        {
            if (i % 2 == 0)
                pgnBuilder.Append($"{i / 2 + 1}. {_chessGame.Moves[i].SAN} ");
            else
                pgnBuilder.Append($"{_chessGame.Moves[i].SAN} ");
        }

        return pgnBuilder.ToString().Trim();
    }

    public void ClaimDraw() => _chessGame.ClaimDraw();

    public void Resign(PlayerColor player) => _chessGame.Resign(player == PlayerColor.White 
        ? Player.White : Player.Black);

    public (PlayerInfoModel? Winner, PlayerInfoModel? Loser) GetWinnerAndLoser()
    {
        if (!_chessGame.IsWinner(Player.White) && !_chessGame.IsWinner(Player.Black))
            return (null, null);
        
        var (winner, loser) = _chessGame.IsWinner(Player.White) 
            ? (WhitePlayerInfo, BlackPlayerInfo)
            : (BlackPlayerInfo, WhitePlayerInfo);
        return (winner, loser);
    }

    public void LoseOnTimeOrThrow()
    {
        lock (_lockObject)
        {
            if (IsOver)
                return;
            if (DateTime.UtcNow < TimeExpiresAt)
                throw new InvalidOperationException($"[{nameof(LoseOnTimeOrThrow)}]: time is not up.");
            Resign(IsWhiteTurn ? PlayerColor.White : PlayerColor.Black);
        }
    }
    
    private void ReduceTime(bool isWhiteTurn)
    {
        var deltaTime = DateTime.UtcNow - LastMoveTime;
        LastMoveTime = DateTime.UtcNow;
        if (isWhiteTurn)
        {
            WhiteTimeLeft -= deltaTime;
            if (WhiteTimeLeft < TimeSpan.Zero)
                WhiteTimeLeft = TimeSpan.Zero;
        }
        else
        {
            BlackTimeLeft -= deltaTime;
            if (BlackTimeLeft < TimeSpan.Zero)
                BlackTimeLeft = TimeSpan.Zero;
        }
    }

    private void DoIncrement(bool isWhiteTurn)
    {
        if (isWhiteTurn)
            WhiteTimeLeft += Increment;
        else
            BlackTimeLeft += Increment;
    }
}