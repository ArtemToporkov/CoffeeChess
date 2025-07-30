using System.Text;
using ChessDotNetCore;
using CoffeeChess.Domain.Entities;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Events;
using CoffeeChess.Domain.Events.Game;
using GameResult = CoffeeChess.Domain.Enums.GameResult;
using PlayerSide = ChessDotNetCore.Player;
namespace CoffeeChess.Domain.Aggregates;

public class Game
{
    public string GameId { get; }
    public string WhitePlayerId { get; }
    public string BlackPlayerId { get; }
    public Chat Chat { get; }
    public bool IsOver { get; private set; }
    public TimeSpan WhiteTimeLeft { get; private set; }
    public TimeSpan BlackTimeLeft { get; private set; }
    public TimeSpan Increment { get; }
    public DateTime LastTimeUpdate { get; private set; }
    public PlayerColor CurrentPlayerColor { get; private set; }
    public PlayerColor? PlayerWithDrawOffer { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    private readonly ChessGame _chessGame = new();
    private readonly Lock _lock = new();
    private readonly List<IDomainEvent> _domainEvents = [];

    public Game(
        string gameId,
        string whitePlayerId,
        string blackPlayerId,
        TimeSpan minutesLeftForPlayer,
        TimeSpan increment)
    {
        GameId = gameId;
        WhitePlayerId = whitePlayerId;
        BlackPlayerId = blackPlayerId;
        WhiteTimeLeft = minutesLeftForPlayer;
        BlackTimeLeft = minutesLeftForPlayer;
        Increment = increment;
        LastTimeUpdate = DateTime.UtcNow;
        CurrentPlayerColor = PlayerColor.White;
        Chat = new();
        IsOver = false;
        _domainEvents.Add(new GameStarted(
            GameId, WhitePlayerId, BlackPlayerId, (int)WhiteTimeLeft.TotalMilliseconds));
    }
    
    public void ClearDomainEvents() => _domainEvents.Clear();
    
    public void ApplyMove(string playerId, string from, string to, string? promotion)
    {
        var playerColor = GetColorById(playerId);
        var currentPlayerId = CurrentPlayerColor == PlayerColor.White 
            ? WhitePlayerId 
            : BlackPlayerId;
        
        if (playerId != currentPlayerId)
        {
            _domainEvents.Add(new MoveFailed(playerId, MoveFailedReason.NotYourTurn));
            return;
        }

        var promotionChar = promotion?[0];

        var move = new Move(from, to, CurrentPlayerColor == PlayerColor.White 
            ? PlayerSide.White : PlayerSide.Black, promotionChar);
        if (_chessGame.MakeMove(move, false) is MoveType.Invalid)
        {
            _domainEvents.Add(new MoveFailed(playerId, MoveFailedReason.InvalidMove));
            return;
        }
        
        if (PlayerWithDrawOffer.HasValue && playerColor != PlayerWithDrawOffer)
            DeclineDrawOffer(playerId);

        if (UpdateTimeAndCheckTimeout())
        {
            _domainEvents.Add(new MoveFailed(playerId, MoveFailedReason.TimeRanOut));
            _chessGame.Resign(CurrentPlayerColor == PlayerColor.White ? PlayerSide.White : PlayerSide.Black);
            var (result, reason) = CurrentPlayerColor == PlayerColor.White 
                ? (GameResult.BlackWon, GameResultReason.WhiteTimeRanOut) 
                : (GameResult.WhiteWon, GameResultReason.BlackTimeRanOut) ;
            _domainEvents.Add(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
            IsOver = true;
            return;
        }

        DoIncrement();
        _domainEvents.Add(new MoveMade(WhitePlayerId, BlackPlayerId, 
            GetPgn(), WhiteTimeLeft, BlackTimeLeft));
        CurrentPlayerColor = CurrentPlayerColor == PlayerColor.White
            ? PlayerColor.Black : PlayerColor.White;
        
        if (_chessGame.ThreeFoldRepeatAndThisCanResultInDraw)
        {
            _domainEvents.Add(new GameResultUpdated(
                WhitePlayerId, BlackPlayerId, GameResult.Draw, GameResultReason.Threefold));
            IsOver = true;
            return;
        }

        if (_chessGame.FiftyMovesAndThisCanResultInDraw)
        {
            _domainEvents.Add(new GameResultUpdated(WhitePlayerId, BlackPlayerId, 
                GameResult.Draw, GameResultReason.FiftyMovesRule));
            IsOver = true;
            return;
        }
        
        if (_chessGame.IsCheckmated(PlayerSide.White) || _chessGame.IsCheckmated(PlayerSide.Black))
        {
            var (result, reason) = _chessGame.IsCheckmated(PlayerSide.White) 
                ? (GameResult.BlackWon, GameResultReason.WhiteCheckmates) 
                : (GameResult.WhiteWon, GameResultReason.BlackCheckmates);
            _domainEvents.Add(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
            IsOver = true;
            return;
        }
        
        if (_chessGame.IsStalemated(PlayerSide.White) || _chessGame.IsStalemated(PlayerSide.Black))
        { 
            _domainEvents.Add(new GameResultUpdated(WhitePlayerId,  BlackPlayerId, 
                GameResult.Draw, GameResultReason.Stalemate));
            IsOver = true;
        }
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
    
    public PlayerColor GetColorById(string playerId)
    {
        if (playerId == WhitePlayerId)
            return PlayerColor.White;
        if (playerId == BlackPlayerId)
            return PlayerColor.Black;
        throw new InvalidOperationException("There's no such player in the game.");
    }
    
    public void OfferADraw(string playerId)
    {
        if (PlayerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's already pending draw offer.");
        PlayerWithDrawOffer = playerId == WhitePlayerId ? PlayerColor.White : PlayerColor.Black;
        var senderColor = GetColorById(playerId);
        var (senderId, receiverId) = senderColor == PlayerColor.White
            ? (WhitePlayerId, BlackPlayerId) 
            : (BlackPlayerId, WhitePlayerId);
        _domainEvents.Add(new DrawOfferSent(senderId, receiverId));
    }

    public void AcceptDrawOffer(string playerId)
    {
        if (!PlayerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (PlayerWithDrawOffer == playerColor)
            throw new InvalidOperationException("The same side tries to offer and accept a draw.");
        PlayerWithDrawOffer = null;
        _domainEvents.Add(new GameResultUpdated(
            WhitePlayerId, BlackPlayerId, GameResult.Draw, GameResultReason.Agreement));
        IsOver = true;
    }

    public void DeclineDrawOffer(string playerId)
    {
        if (!PlayerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (PlayerWithDrawOffer == playerColor)
            throw new InvalidOperationException("The same side tries to offer and decline a draw.");
        PlayerWithDrawOffer = null;
        var (rejectingId, senderId) = PlayerWithDrawOffer == PlayerColor.White
            ? (BlackPlayerId, WhitePlayerId)
            : (WhitePlayerId, BlackPlayerId);
        _domainEvents.Add(new DrawOfferDeclined(rejectingId, senderId));
    }

    public void Resign(string playerId) 
    {
        var isWhite = GetColorById(playerId) == PlayerColor.White;
        _chessGame.Resign(isWhite ? PlayerSide.White : PlayerSide.Black);
        var (result, reason) = isWhite 
            ? (GameResult.BlackWon, GameResultReason.WhiteResigns) 
            : (GameResult.WhiteWon, GameResultReason.BlackResigns);
        _domainEvents.Add(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
        IsOver = true;
    }

    public void CheckTimeout()
    {
        if (UpdateTimeAndCheckTimeout())
        {
            _chessGame.Resign(CurrentPlayerColor == PlayerColor.White ? PlayerSide.White : PlayerSide.Black);
            var (result, reason) = CurrentPlayerColor == PlayerColor.White
                ? (GameResult.BlackWon, GameResultReason.WhiteTimeRanOut)
                : (GameResult.WhiteWon, GameResultReason.BlackTimeRanOut);
            _domainEvents.Add(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
            IsOver = true;
        }
    }

    private bool UpdateTimeAndCheckTimeout()
    {
        lock (_lock)
        {
            var deltaTime = DateTime.UtcNow - LastTimeUpdate;
            LastTimeUpdate = DateTime.UtcNow;
            if (CurrentPlayerColor is PlayerColor.White)
            {
                WhiteTimeLeft -= deltaTime;
                if (WhiteTimeLeft >= TimeSpan.Zero) 
                    return false;
            
                WhiteTimeLeft = TimeSpan.Zero;
            }
            else
            {
                BlackTimeLeft -= deltaTime;
                if (BlackTimeLeft >= TimeSpan.Zero) 
                    return false;
            
                BlackTimeLeft = TimeSpan.Zero;
            }

            return true;   
        }
    }

    private void DoIncrement()
    {
        if (CurrentPlayerColor is PlayerColor.White)
            WhiteTimeLeft += Increment;
        else
            BlackTimeLeft += Increment;
    }
}