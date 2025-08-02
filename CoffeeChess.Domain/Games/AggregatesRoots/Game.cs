using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Games.ValueObjects;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;
using GameResult = CoffeeChess.Domain.Games.Enums.GameResult;
using MoveType = CoffeeChess.Domain.Games.Enums.MoveType;

namespace CoffeeChess.Domain.Games.AggregatesRoots;

public class Game : AggregateRoot<IDomainEvent>
{
    [JsonInclude] public string GameId { get; init; } = null!;
    [JsonInclude] public string WhitePlayerId { get; init; } = null!;
    [JsonInclude] public string BlackPlayerId { get; init; } = null!;
    [JsonInclude] public bool IsOver { get; private set; }

    [JsonInclude] private TimeSpan _whiteTimeLeft;
    [JsonInclude] private TimeSpan _blackTimeLeft;
    [JsonInclude] private TimeSpan _increment;
    [JsonInclude] private DateTime _lastTimeUpdate;
    [JsonInclude] private PlayerColor _currentPlayerColor;
    [JsonInclude] private PlayerColor? _playerWithDrawOffer;
    // TODO: create and use struct Fen instead
    [JsonInclude] private Fen _currentFen;
    [JsonInclude] private Dictionary<Fen, int> _positionsForThreefoldCount = null!;
    // TODO: create and use struct SanMove instead
    [JsonInclude] private List<SanMove> _sanMovesHistory = null!;

    public Game(
        string gameId,
        string whitePlayerId, string blackPlayerId,
        TimeSpan minutesLeftForPlayer, TimeSpan increment)
    {
        GameId = gameId;
        WhitePlayerId = whitePlayerId;
        BlackPlayerId = blackPlayerId;
        _whiteTimeLeft = minutesLeftForPlayer;
        _blackTimeLeft = minutesLeftForPlayer;
        _increment = increment;
        _lastTimeUpdate = DateTime.UtcNow;
        _currentPlayerColor = PlayerColor.White;
        IsOver = false;
        _positionsForThreefoldCount = new();
        _sanMovesHistory = new();
        _currentFen = new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        AddDomainEvent(new GameStarted(
            GameId, WhitePlayerId, BlackPlayerId, (int)_whiteTimeLeft.TotalMilliseconds));
    }

    [JsonConstructor] private Game() { }

    public void ApplyMove(IChessMovesValidator chessMovesValidator, 
        string playerId, string from, string to, string? promotion)
    {
        if (CheckAndPublishNotYourTurn(playerId)) return;

        var moveResult = chessMovesValidator.ApplyMove(_currentFen, _currentPlayerColor, from, to, promotion?[0]);
        if (CheckAndPublishInvalidMove(playerId, moveResult)) return;
        
        DeclineDrawOfferIfPending(playerId);
        ReduceTime();

        if (CheckAndPublishTimeout(movingPlayerId: playerId)) return;
        
        UpdateAndPublishAfterSuccessMove(moveResult);
        DoIncrement();

        if (CheckAndPublishThreefold(moveResult)) return;
        if (CheckAndPublishCheckmate(moveResult)) return;
        if (CheckAndPublishStalemate(moveResult)) return;
        // TODO: implement 50-moves rule
        
        SwapColors();
    }

    public void OfferADraw(string playerId)
    {
        if (_playerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's already pending draw offer.");
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
            throw new InvalidOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (_playerWithDrawOffer == playerColor)
            throw new InvalidOperationException("The same side tries to offer and accept a draw.");
        _playerWithDrawOffer = null;
        AddDomainEvent(new GameResultUpdated(
            WhitePlayerId, BlackPlayerId, GameResult.Draw, GameResultReason.Agreement));
        IsOver = true;
    }

    public void DeclineDrawOffer(string playerId)
    {
        if (!_playerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (_playerWithDrawOffer == playerColor)
            throw new InvalidOperationException("The same side tries to offer and decline a draw.");
        _playerWithDrawOffer = null;
        var (rejectingId, senderId) = _playerWithDrawOffer == PlayerColor.White
            ? (BlackPlayerId, WhitePlayerId)
            : (WhitePlayerId, BlackPlayerId);
        AddDomainEvent(new DrawOfferDeclined(rejectingId, senderId));
    }

    public void Resign(string playerId)
    {
        var isWhite = GetColorById(playerId) == PlayerColor.White;
        var (result, reason) = isWhite
            ? (GameResult.BlackWon, GameResultReason.WhiteResigns)
            : (GameResult.WhiteWon, GameResultReason.BlackResigns);
        AddDomainEvent(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
        IsOver = true;
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
        var deltaTime = DateTime.UtcNow - _lastTimeUpdate;
        _lastTimeUpdate = DateTime.UtcNow;
        if (_currentPlayerColor is PlayerColor.White)
            _whiteTimeLeft -= deltaTime;
        else
            _blackTimeLeft -= deltaTime;
    }

    private bool CheckAndPublishCheckmate(MoveResult moveResult)
    {
        if (moveResult.MoveResultType is not MoveResultType.Checkmate)
            return false;
        
        var (result, reason) = _currentPlayerColor == PlayerColor.White
            ? (GameResult.WhiteWon, GameResultReason.WhiteCheckmates)
            : (GameResult.BlackWon, GameResultReason.BlackCheckmates);
        EndGameAndPublish(result, reason);
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
            EndGameAndPublish(GameResult.BlackWon, GameResultReason.WhiteTimeRanOut);
        }
        else
        {
            if (_blackTimeLeft > TimeSpan.Zero)
                return false;
            
            if (!string.IsNullOrEmpty(movingPlayerId))
                AddDomainEvent(new MoveFailed(movingPlayerId, MoveFailedReason.TimeRanOut));
            EndGameAndPublish(GameResult.WhiteWon, GameResultReason.BlackTimeRanOut);
            _blackTimeLeft = TimeSpan.Zero;
        }
        return true;
    }

    private void UpdateAndPublishAfterSuccessMove(MoveResult moveResult)
    {
        _sanMovesHistory.Add(moveResult.San!.Value);
        _currentFen = moveResult.FenAfterMove!.Value;
        AddDomainEvent(new MoveMade(WhitePlayerId, BlackPlayerId,
            _sanMovesHistory.AsReadOnly(), _whiteTimeLeft, _blackTimeLeft));
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
        AddDomainEvent(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
        IsOver = true;
    }
    
    private bool CheckAndPublishThreefold(MoveResult moveResult)
    {
        // TODO: also check if it's neither pawn nor promotion
        if (moveResult.MoveType is not MoveType.Capture)
        {
            _positionsForThreefoldCount[_currentFen]++;
            if (_positionsForThreefoldCount[_currentFen] == 3)
            {
                AddDomainEvent(new GameResultUpdated(
                    WhitePlayerId, BlackPlayerId, GameResult.Draw, GameResultReason.Threefold));
                IsOver = true;
                return true;
            }
        }
        else 
            _positionsForThreefoldCount.Clear();

        return false;
    }
    
    private void DoIncrement()
    {
        if (_currentPlayerColor is PlayerColor.White)
            _whiteTimeLeft += _increment;
        else
            _blackTimeLeft += _increment;
    }

    private PlayerColor GetColorById(string playerId)
    {
        if (playerId == WhitePlayerId)
            return PlayerColor.White;
        if (playerId == BlackPlayerId)
            return PlayerColor.Black;
        throw new InvalidOperationException("There's no such player in the game.");
    }
}