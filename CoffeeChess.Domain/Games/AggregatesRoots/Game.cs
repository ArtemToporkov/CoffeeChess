using System.Text.Json.Serialization;
using ChessDotNetCore;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Domain.Shared.Abstractions;
using CoffeeChess.Domain.Shared.Interfaces;
using GameResult = CoffeeChess.Domain.Games.Enums.GameResult;
using MoveType = CoffeeChess.Domain.Games.Enums.MoveType;
using PlayerSide = ChessDotNetCore.Player;

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
    [JsonInclude] private string _currentFen = null!;
    [JsonInclude] private Dictionary<string, int> _positionsForThreefoldCount = null!;

    // TODO: use MoveInfo class instead of string
    [JsonInclude] private List<string> _sanMovesHistory = null!;
    // TODO: apply dependency inversion by creating IChessRules
    // NOTE: done for optimization to check some properties without initializing ChessGame

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
        _currentFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        AddDomainEvent(new GameStarted(
            GameId, WhitePlayerId, BlackPlayerId, (int)_whiteTimeLeft.TotalMilliseconds));
    }

    [JsonConstructor] private Game() { }

    public void ApplyMove(IChessRules chessRules, string playerId, string from, string to, string? promotion)
    {
        var playerColor = GetColorById(playerId);
        var currentPlayerId = _currentPlayerColor == PlayerColor.White
            ? WhitePlayerId
            : BlackPlayerId;

        if (playerId != currentPlayerId)
        {
            AddDomainEvent(new MoveFailed(playerId, MoveFailedReason.NotYourTurn));
            return;
        }

        var promotionChar = promotion?[0];
        var moveResult = chessRules.ApplyMove(_currentFen, _currentPlayerColor, from, to, promotionChar);
        if (!moveResult.Valid)
        {
            AddDomainEvent(new MoveFailed(playerId, MoveFailedReason.InvalidMove));
            return;
        }

        if (_playerWithDrawOffer.HasValue && playerColor != _playerWithDrawOffer)
            DeclineDrawOffer(playerId);

        if (UpdateTimeAndCheckTimeout())
        {
            AddDomainEvent(new MoveFailed(playerId, MoveFailedReason.TimeRanOut));
            var (result, reason) = _currentPlayerColor == PlayerColor.White
                ? (GameResult.BlackWon, GameResultReason.WhiteTimeRanOut)
                : (GameResult.WhiteWon, GameResultReason.BlackTimeRanOut);
            AddDomainEvent(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
            IsOver = true;
            return;
        }

        DoIncrement();
        _sanMovesHistory.Add(moveResult.San);
        _currentFen = moveResult.FenAfterMove;
        AddDomainEvent(new MoveMade(WhitePlayerId, BlackPlayerId,
            _sanMovesHistory.AsReadOnly(), _whiteTimeLeft, _blackTimeLeft));

        _currentPlayerColor = _currentPlayerColor == PlayerColor.White
            ? PlayerColor.Black
            : PlayerColor.White;

        if (CheckThreefold(moveResult.MoveType is MoveType.Capture))
            return;

        // TODO: implement 50-moves rule
        /*if (FiftyMovesAndThisCanResultInDraw)
        {
            AddDomainEvent(new GameResultUpdated(WhitePlayerId, BlackPlayerId,
                GameResult.Draw, GameResultReason.FiftyMovesRule));
            IsOver = true;
            return;
        }*/

        if (moveResult.MoveResultType is MoveResultType.Checkmate)
        {
            var (result, reason) = _currentPlayerColor == PlayerColor.White
                ? (GameResult.WhiteWon, GameResultReason.WhiteCheckmates)
                : (GameResult.BlackWon, GameResultReason.BlackCheckmates);
            AddDomainEvent(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
            IsOver = true;
            return;
        }

        if (moveResult.MoveResultType is MoveResultType.Stalemate)
        {
            AddDomainEvent(new GameResultUpdated(WhitePlayerId, BlackPlayerId,
                GameResult.Draw, GameResultReason.Stalemate));
            IsOver = true;
        }
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
        if (UpdateTimeAndCheckTimeout())
        {
            var (result, reason) = _currentPlayerColor == PlayerColor.White
                ? (GameResult.BlackWon, GameResultReason.WhiteTimeRanOut)
                : (GameResult.WhiteWon, GameResultReason.BlackTimeRanOut);
            AddDomainEvent(new GameResultUpdated(WhitePlayerId, BlackPlayerId, result, reason));
            IsOver = true;
        }
    }

    private bool CheckThreefold(bool wasCapture)
    {
        if (!wasCapture)
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

    private bool UpdateTimeAndCheckTimeout()
    {
        var deltaTime = DateTime.UtcNow - _lastTimeUpdate;
        _lastTimeUpdate = DateTime.UtcNow;
        if (_currentPlayerColor is PlayerColor.White)
        {
            _whiteTimeLeft -= deltaTime;
            if (_whiteTimeLeft >= TimeSpan.Zero)
                return false;

            _whiteTimeLeft = TimeSpan.Zero;
        }
        else
        {
            _blackTimeLeft -= deltaTime;
            if (_blackTimeLeft >= TimeSpan.Zero)
                return false;

            _blackTimeLeft = TimeSpan.Zero;
        }

        return true;
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