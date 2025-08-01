using ChessDotNetCore;
using CoffeeChess.Domain.Games.Entities;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Shared.Interfaces;
using GameResult = CoffeeChess.Domain.Games.Enums.GameResult;
using PlayerSide = ChessDotNetCore.Player;
namespace CoffeeChess.Domain.Games.AggregatesRoots;

public class Game
{
    public string GameId { get; }
    public string WhitePlayerId { get; }
    public string BlackPlayerId { get; }
    public bool IsOver { get; private set; }
    public TimeSpan WhiteTimeLeft { get; private set; }
    public TimeSpan BlackTimeLeft { get; private set; }
    public TimeSpan Increment { get; }
    public DateTime LastTimeUpdate { get; private set; }
    public PlayerColor CurrentPlayerColor { get; private set; }
    public PlayerColor? PlayerWithDrawOffer { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    // TODO: use MoveInfo class instead of string
    public IReadOnlyCollection<string> SanMovesHistory => _sanMovesHistory.AsReadOnly();

    private string _currentFenPosition;
    private readonly List<string> _sanMovesHistory;
    private readonly ChessGame _chessGame;
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
        IsOver = false;
        _chessGame = new();
        _sanMovesHistory = new();
        _currentFenPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        _domainEvents.Add(new GameStarted(
            GameId, WhitePlayerId, BlackPlayerId, (int)WhiteTimeLeft.TotalMilliseconds));
    }

    public Game(GameState gameState)
    {
        _chessGame = new(gameState.CurrentFenPosition);
        GameId = gameState.GameId;
        WhitePlayerId = gameState.WhitePlayerId;
        BlackPlayerId = gameState.BlackPlayerId;
        IsOver = gameState.IsOver;
        WhiteTimeLeft = gameState.WhiteTimeLeft;
        BlackTimeLeft = gameState.BlackTimeLeft;
        Increment = gameState.Increment;
        LastTimeUpdate = gameState.LastTimeUpdate;
        CurrentPlayerColor = gameState.CurrentPlayerColor;
        PlayerWithDrawOffer = gameState.PlayerWithDrawOffer;
        _currentFenPosition = gameState.CurrentFenPosition;
        _sanMovesHistory = gameState.SanMovesHistory.ToList();
    }

    public GameState GetGameState()
    {
        return new GameState
        {
            GameId = GameId,
            WhitePlayerId = WhitePlayerId,
            BlackPlayerId = BlackPlayerId,
            IsOver = IsOver,
            WhiteTimeLeft = WhiteTimeLeft,
            BlackTimeLeft = BlackTimeLeft,
            Increment = Increment,
            LastTimeUpdate = LastTimeUpdate,
            CurrentPlayerColor = CurrentPlayerColor,
            PlayerWithDrawOffer = PlayerWithDrawOffer,
            CurrentFenPosition = _currentFenPosition,
            SanMovesHistory = SanMovesHistory
        };
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
        _sanMovesHistory.Add(_chessGame.LastMove!.SAN);
        _currentFenPosition = _chessGame.GetFen();
        _domainEvents.Add(new MoveMade(WhitePlayerId, BlackPlayerId, 
            SanMovesHistory, WhiteTimeLeft, BlackTimeLeft));
        
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