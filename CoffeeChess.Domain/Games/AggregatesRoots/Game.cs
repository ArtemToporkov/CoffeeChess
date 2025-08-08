using System.Text.Json.Serialization;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Exceptions;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;
using GameResult = CoffeeChess.Domain.Games.Enums.GameResult;

namespace CoffeeChess.Domain.Games.AggregatesRoots;

public class Game : AggregateRoot<IDomainEvent>
{
    public string GameId { get; init; } = null!;
    public string WhitePlayerId { get; init; } = null!;
    public string BlackPlayerId { get; init; } = null!;
    public bool IsOver { get; private set; }
    public TimeSpan InitialTimeForOnePlayer { get; init; }
    public TimeSpan Increment { get; init; }
    public DateTime LastTimeUpdate { get; private set; }
    public IReadOnlyList<MoveInfo> MovesHistory => _movesHistory.AsReadOnly();

    private TimeSpan _whiteTimeLeft;
    private TimeSpan _blackTimeLeft;
    private PlayerColor _currentPlayerColor;
    private PlayerColor? _playerWithDrawOffer;
    private Fen _currentFen;
    private Dictionary<string, int> _positionsForThreefoldCount = null!;
    private List<MoveInfo> _movesHistory = null!;

    public Game(
        string gameId,
        string whitePlayerId, string blackPlayerId,
        TimeSpan minutesLeftForPlayer, TimeSpan increment)
    {
        GameId = gameId;
        WhitePlayerId = whitePlayerId;
        BlackPlayerId = blackPlayerId;
        IsOver = false;
        InitialTimeForOnePlayer = minutesLeftForPlayer;
        Increment = increment;
        LastTimeUpdate = DateTime.UtcNow;
        
        _whiteTimeLeft = InitialTimeForOnePlayer;
        _blackTimeLeft = InitialTimeForOnePlayer;
        _currentPlayerColor = PlayerColor.White;
        _positionsForThreefoldCount = new();
        _movesHistory = new();
        _currentFen = new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        AddDomainEvent(new GameStarted(
            GameId, WhitePlayerId, BlackPlayerId, (int)_whiteTimeLeft.TotalMilliseconds));
    }

    public void ApplyMove(IChessMovesValidator chessMovesValidator, 
        string playerId, ChessSquare from, ChessSquare to, Promotion? promotion)
    {
        if (IsOver) throw new InvalidGameOperationException("Game is over.");
        if (CheckAndPublishNotYourTurn(playerId)) return;

        var moveResult = chessMovesValidator.ApplyMove(_currentFen, _currentPlayerColor, from, to, promotion);
        if (CheckAndPublishInvalidMove(playerId, moveResult)) return;
        
        DeclineDrawOfferIfPending(playerId);
        ReduceTime();

        if (CheckAndPublishTimeout(movingPlayerId: playerId)) return;
        
        DoIncrement();
        UpdateAndPublishAfterSuccessMove(moveResult);

        if (CheckAndPublishCheckmate(moveResult)) return;
        if (CheckAndPublishStalemate(moveResult)) return;
        if (CheckAndPublishFiftyMovesRule(moveResult)) return;
        if (CheckAndPublishThreefold(moveResult)) return;
        
        SwapColors();
    }

    public void OfferADraw(string playerId)
    {
        if (_playerWithDrawOffer.HasValue)
            throw new InvalidGameOperationException("There's already pending draw offer.");
        _playerWithDrawOffer = playerId == WhitePlayerId ? PlayerColor.White : PlayerColor.Black;
        var senderColor = GetColorById(playerId);
        var (senderId, receiverId) = senderColor == PlayerColor.White
            ? (WhitePlayerId, BlackPlayerId)
            : (BlackPlayerId, WhitePlayerId);
        AddDomainEvent(new DrawOfferSent(senderId, receiverId));
    }

    public void AcceptDrawOffer(string playerId)
    {
        if (!_playerWithDrawOffer.HasValue)
            throw new InvalidGameOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (_playerWithDrawOffer == playerColor)
            throw new InvalidGameOperationException("The same side tries to offer and accept a draw.");
        _playerWithDrawOffer = null;
        EndGameAndPublish(GameResult.Draw, GameResultReason.Agreement);
    }

    public void DeclineDrawOffer(string playerId)
    {
        if (!_playerWithDrawOffer.HasValue)
            throw new InvalidGameOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (_playerWithDrawOffer == playerColor)
            throw new InvalidGameOperationException("The same side tries to offer and decline a draw.");
        _playerWithDrawOffer = null;
        var (rejectingId, senderId) = _playerWithDrawOffer == PlayerColor.White
            ? (BlackPlayerId, WhitePlayerId)
            : (WhitePlayerId, BlackPlayerId);
        AddDomainEvent(new DrawOfferDeclined(rejectingId, senderId));
    }

    public void Resign(string playerId)
    {
        var isWhite = GetColorById(playerId) == PlayerColor.White;
        var result = isWhite ? GameResult.BlackWon : GameResult.WhiteWon;
        EndGameAndPublish(result, GameResultReason.OpponentResigned);
    }

    public void CheckTimeout()
    {
        ReduceTime();
        CheckAndPublishTimeout();
    }

    private bool CheckAndPublishNotYourTurn(string playerId)
    {
        var playerColor = GetColorById(playerId);
        if (playerColor == _currentPlayerColor) 
            return false;
        
        AddDomainEvent(new MoveFailed(playerId, MoveFailedReason.NotYourTurn));
        return true;
    }

    private void ReduceTime()
    {
        var deltaTime = DateTime.UtcNow - LastTimeUpdate;
        LastTimeUpdate = DateTime.UtcNow;
        if (_currentPlayerColor is PlayerColor.White)
            _whiteTimeLeft -= deltaTime;
        else
            _blackTimeLeft -= deltaTime;
    }

    private bool CheckAndPublishCheckmate(MoveResult moveResult)
    {
        if (moveResult.MoveResultType is not MoveResultType.Checkmate)
            return false;

        var result = _currentPlayerColor == PlayerColor.White ? GameResult.WhiteWon : GameResult.BlackWon;
        EndGameAndPublish(result, GameResultReason.Checkmate);
        return true;
    }
    
    private bool CheckAndPublishStalemate(MoveResult moveResult)
    {
        if (moveResult.MoveResultType is not MoveResultType.Stalemate)
            return false;
        
        EndGameAndPublish(GameResult.Draw, GameResultReason.Stalemate);
        return true;
    }

    private bool CheckAndPublishInvalidMove(string playerId, MoveResult moveResult)
    {
        if (moveResult.Valid)
            return false;
        
        AddDomainEvent(new MoveFailed(playerId, MoveFailedReason.InvalidMove));
        return true;
    }
    
    private bool CheckAndPublishTimeout(string? movingPlayerId = null)
    {
        if (_currentPlayerColor is PlayerColor.White)
        {
            if (_whiteTimeLeft > TimeSpan.Zero)
                return false;
            _whiteTimeLeft = TimeSpan.Zero;
            if (!string.IsNullOrEmpty(movingPlayerId))
                AddDomainEvent(new MoveFailed(movingPlayerId, MoveFailedReason.TimeRanOut));
            EndGameAndPublish(GameResult.BlackWon, GameResultReason.OpponentTimeRanOut);
        }
        else
        {
            if (_blackTimeLeft > TimeSpan.Zero)
                return false;
            
            if (!string.IsNullOrEmpty(movingPlayerId))
                AddDomainEvent(new MoveFailed(movingPlayerId, MoveFailedReason.TimeRanOut));
            EndGameAndPublish(GameResult.WhiteWon, GameResultReason.OpponentTimeRanOut);
            _blackTimeLeft = TimeSpan.Zero;
        }
        return true;
    }

    private void UpdateAndPublishAfterSuccessMove(MoveResult moveResult)
    {
        var timeAfterMove = _currentPlayerColor == PlayerColor.White ? _whiteTimeLeft : _blackTimeLeft;
        _movesHistory.Add(new(moveResult.San!.Value, timeAfterMove));
        _currentFen = moveResult.FenAfterMove!.Value;
        AddDomainEvent(new MoveMade(WhitePlayerId, BlackPlayerId,
            _movesHistory.AsReadOnly(), _whiteTimeLeft, _blackTimeLeft));
    }
    private void DeclineDrawOfferIfPending(string playerId)
    {
        if (_playerWithDrawOffer.HasValue && GetColorById(playerId) != _playerWithDrawOffer)
            DeclineDrawOffer(playerId);
    }

    private void SwapColors() 
        => _currentPlayerColor = _currentPlayerColor == PlayerColor.White
            ? PlayerColor.Black
            : PlayerColor.White;

    private void EndGameAndPublish(GameResult result, GameResultReason reason)
    {
        AddDomainEvent(new GameResultUpdated(GameId, WhitePlayerId, BlackPlayerId, result, reason));
        IsOver = true;
    }
    
    private bool CheckAndPublishThreefold(MoveResult moveResult)
    {
        if (!moveResult.IsCaptureOrPawnMove!.Value)
        {
            _positionsForThreefoldCount.TryAdd(_currentFen.PiecesPlacement, 0);
            _positionsForThreefoldCount[_currentFen.PiecesPlacement]++;
            if (_positionsForThreefoldCount[_currentFen.PiecesPlacement] == 3)
            {
                EndGameAndPublish(GameResult.Draw, GameResultReason.Threefold);
                return true;
            }
        }
        else 
            _positionsForThreefoldCount.Clear();

        return false;
    }

    private bool CheckAndPublishFiftyMovesRule(MoveResult moveResult)
    {
        if (moveResult.FenAfterMove!.Value.PliesCount < 100)
            return false;
        
        EndGameAndPublish(GameResult.Draw, GameResultReason.FiftyMovesRule);
        return true;
    }
    
    private void DoIncrement()
    {
        if (_currentPlayerColor is PlayerColor.White)
            _whiteTimeLeft += Increment;
        else
            _blackTimeLeft += Increment;
    }

    private PlayerColor GetColorById(string playerId)
    {
        if (playerId == WhitePlayerId)
            return PlayerColor.White;
        if (playerId == BlackPlayerId)
            return PlayerColor.Black;
        throw new InvalidGameOperationException("There's no such player in the game.");
    }
}