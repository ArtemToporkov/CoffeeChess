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
    public string GameId { get; } = gameId;
    public PlayerInfoModel WhitePlayerInfo { get; } = whitePlayerInfo;
    public PlayerInfoModel BlackPlayerInfo { get; } = blackPlayerInfo;
    public bool IsOver => _chessGame.GameResult != GameResult.OnGoing &&
                          _chessGame.GameResult != GameResult.Check;
    public TimeSpan WhiteTimeLeft { get; private set; } = minutesLeftForPlayer;
    public TimeSpan BlackTimeLeft { get; private set; } = minutesLeftForPlayer;
    public TimeSpan Increment { get; } = increment;
    public DateTime LastTimeUpdate { get; set; } = DateTime.UtcNow;
    public ConcurrentQueue<ChatMessageModel> ChatMessages { get; } = new();
    public PlayerColor CurrentPlayerColor => _chessGame.CurrentPlayer == Player.White 
        ? PlayerColor.White 
        : PlayerColor.Black;

    private PlayerColor? PlayerWithDrawOffer { get; set; }
    private readonly ChessGame _chessGame = new();
    private readonly Lock _lockObject = new();

    public MoveResult MakeMove(string playerId, string from, string to, string? promotion)
    {
        var currentPlayerId = CurrentPlayerColor == PlayerColor.White 
            ? WhitePlayerInfo.Id 
            : BlackPlayerInfo.Id;
        var currentPlayerColor = CurrentPlayerColor;
        
        if (playerId != currentPlayerId)
            return MoveResult.NotYourTurn;

        var promotionChar = promotion?[0];

        var move = new Move(from, to, CurrentPlayerColor == PlayerColor.White 
            ? Player.White : Player.Black, promotionChar);
        if (_chessGame.MakeMove(move, false) is MoveType.Invalid)
            return MoveResult.Invalid;

        if (UpdateTimeAndCheckTimeout(currentPlayerColor))
        {
            Resign(currentPlayerColor);
            return MoveResult.TimeRanOut;
        }

        if (_chessGame.ThreeFoldRepeatAndThisCanResultInDraw)
            return MoveResult.ThreeFold;

        if (_chessGame.FiftyMovesAndThisCanResultInDraw)
            return MoveResult.FiftyMovesRule;
        
        if (_chessGame.IsCheckmated(Player.White) || _chessGame.IsCheckmated(Player.Black))
            return MoveResult.Checkmate;
        
        if (_chessGame.IsStalemated(Player.White) || _chessGame.IsStalemated(Player.Black))
            return MoveResult.Stalemate;
        
        DoIncrement(currentPlayerColor);
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
    
    public DrawOfferResult SendDrawOffer(PlayerColor playerColor)
    {
        if (PlayerWithDrawOffer.HasValue)
            return DrawOfferResult.Fail("Draw offer already exists.");
        PlayerWithDrawOffer = playerColor;
        return DrawOfferResult.Ok();
    }

    public DrawOfferResult AcceptDrawOffer(PlayerColor playerColor)
    {
        if (!PlayerWithDrawOffer.HasValue)
            return DrawOfferResult.Fail("There's no pending draw offers.");
        if (PlayerWithDrawOffer == playerColor)
            return DrawOfferResult.Fail("The same side tries to offer and accept a draw.");
        ClaimDraw();
        PlayerWithDrawOffer = null;
        return DrawOfferResult.Ok();
    }

    public DrawOfferResult DeclineDrawOffer(PlayerColor playerColor)
    {
        if (!PlayerWithDrawOffer.HasValue)
            return DrawOfferResult.Fail("There's no pending draw offers.");
        if (PlayerWithDrawOffer == playerColor)
            return DrawOfferResult.Fail("The same side tries to offer and decline a draw.");
        PlayerWithDrawOffer = null;
        return DrawOfferResult.Ok();
    }
    
    public PlayerColor? GetColorById(string playerId)
    {
        if (playerId == WhitePlayerInfo.Id)
            return PlayerColor.White;
        if (playerId == BlackPlayerInfo.Id)
            return PlayerColor.Black;
        return null;
    }

    public void ClearDrawOffer() => PlayerWithDrawOffer = null;

    public bool HasPendingDrawOffer() => PlayerWithDrawOffer.HasValue;

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

    public bool UpdateTimeAndCheckTimeout(PlayerColor playerColor)
    {
        lock (_lockObject)
        {
            var deltaTime = DateTime.UtcNow - LastTimeUpdate;
            LastTimeUpdate = DateTime.UtcNow;
            if (playerColor is PlayerColor.White)
            {
                WhiteTimeLeft -= deltaTime;
                if (WhiteTimeLeft >= TimeSpan.Zero) return false;
            
                WhiteTimeLeft = TimeSpan.Zero;
            }
            else
            {
                BlackTimeLeft -= deltaTime;
                if (BlackTimeLeft >= TimeSpan.Zero) return false;
            
                BlackTimeLeft = TimeSpan.Zero;
            }

            return true;   
        }
    }

    private void DoIncrement(PlayerColor playerColor)
    {
        if (playerColor is PlayerColor.White)
            WhiteTimeLeft += Increment;
        else
            BlackTimeLeft += Increment;
    }
}