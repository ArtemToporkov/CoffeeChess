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
    public string GameId { get; }
    public string WhitePlayerId { get; }
    public string BlackPlayerId { get; }
    public bool IsOver { get; private set; }
    public TimeSpan InitialTimeForOnePlayer { get; }
    public TimeSpan Increment { get; }
    public DateTime LastTimeUpdate { get; private set; }
    public IReadOnlyList<MoveInfo> MovesHistory => _movesHistory.AsReadOnly();
    public TimeSpan WhiteTimeLeft { get; private set; }
    public TimeSpan BlackTimeLeft { get; private set; }
    public PlayerColor CurrentPlayerColor { get; private set; }
    public PlayerColor? PlayerWithDrawOffer { get; private set; }
    public Fen CurrentFen { get; private set; }
    
    private readonly Dictionary<string, int> _positionsForThreefoldCount;
    private readonly List<MoveInfo> _movesHistory;

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
        
        WhiteTimeLeft = InitialTimeForOnePlayer;
        BlackTimeLeft = InitialTimeForOnePlayer;
        CurrentPlayerColor = PlayerColor.White;
        _positionsForThreefoldCount = new();
        _movesHistory = new();
        CurrentFen = new Fen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        AddDomainEvent(new GameStarted(
            GameId, WhitePlayerId, BlackPlayerId, (int)WhiteTimeLeft.TotalMilliseconds));
    }

    public void ApplyMove(IChessMovesValidatorService chessMovesValidatorService, 
        string playerId, ChessSquare from, ChessSquare to, Promotion? promotion)
    {
        if (IsOver) throw new InvalidGameOperationException("Game is over.");
        if (CheckAndPublishNotYourTurn(playerId)) return;

        var moveResult = chessMovesValidatorService.ApplyMove(CurrentFen, CurrentPlayerColor, from, to, promotion);
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
        if (PlayerWithDrawOffer.HasValue)
            throw new InvalidGameOperationException("There's already pending draw offer.");
        PlayerWithDrawOffer = playerId == WhitePlayerId ? PlayerColor.White : PlayerColor.Black;
        var senderColor = GetColorById(playerId);
        var (senderId, receiverId) = senderColor == PlayerColor.White
            ? (WhitePlayerId, BlackPlayerId)
            : (BlackPlayerId, WhitePlayerId);
        AddDomainEvent(new DrawOfferSent(senderId, receiverId));
    }

    public void AcceptDrawOffer(string playerId)
    {
        if (!PlayerWithDrawOffer.HasValue)
            throw new InvalidGameOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (PlayerWithDrawOffer == playerColor)
            throw new InvalidGameOperationException("The same side tries to offer and accept a draw.");
        PlayerWithDrawOffer = null;
        EndGameAndPublish(GameResult.Draw, GameResultReason.Agreement);
    }

    public void DeclineDrawOffer(string playerId)
    {
        if (!PlayerWithDrawOffer.HasValue)
            throw new InvalidGameOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (PlayerWithDrawOffer == playerColor)
            throw new InvalidGameOperationException("The same side tries to offer and decline a draw.");
        PlayerWithDrawOffer = null;
        var (rejectingId, senderId) = PlayerWithDrawOffer == PlayerColor.White
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
        if (playerColor == CurrentPlayerColor) 
            return false;
        
        AddDomainEvent(new MoveFailed(playerId, MoveFailedReason.NotYourTurn));
        return true;
    }

    private void ReduceTime()
    {
        var deltaTime = DateTime.UtcNow - LastTimeUpdate;
        LastTimeUpdate = DateTime.UtcNow;
        if (CurrentPlayerColor is PlayerColor.White)
            WhiteTimeLeft -= deltaTime;
        else
            BlackTimeLeft -= deltaTime;
    }

    private bool CheckAndPublishCheckmate(MoveResult moveResult)
    {
        if (moveResult.MoveResultType is not MoveResultType.Checkmate)
            return false;

        var result = CurrentPlayerColor == PlayerColor.White ? GameResult.WhiteWon : GameResult.BlackWon;
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
        if (CurrentPlayerColor is PlayerColor.White)
        {
            if (WhiteTimeLeft > TimeSpan.Zero)
                return false;
            WhiteTimeLeft = TimeSpan.Zero;
            if (!string.IsNullOrEmpty(movingPlayerId))
                AddDomainEvent(new MoveFailed(movingPlayerId, MoveFailedReason.TimeRanOut));
            EndGameAndPublish(GameResult.BlackWon, GameResultReason.OpponentTimeRanOut);
        }
        else
        {
            if (BlackTimeLeft > TimeSpan.Zero)
                return false;
            
            if (!string.IsNullOrEmpty(movingPlayerId))
                AddDomainEvent(new MoveFailed(movingPlayerId, MoveFailedReason.TimeRanOut));
            EndGameAndPublish(GameResult.WhiteWon, GameResultReason.OpponentTimeRanOut);
            BlackTimeLeft = TimeSpan.Zero;
        }
        return true;
    }

    private void UpdateAndPublishAfterSuccessMove(MoveResult moveResult)
    {
        var timeAfterMove = CurrentPlayerColor == PlayerColor.White ? WhiteTimeLeft : BlackTimeLeft;
        var moveInfo = new MoveInfo(moveResult.San!.Value, timeAfterMove);
        _movesHistory.Add(moveInfo);
        CurrentFen = moveResult.FenAfterMove!.Value;
        AddDomainEvent(new MoveMade(
            WhitePlayerId, BlackPlayerId, MovesHistory, moveInfo, WhiteTimeLeft, BlackTimeLeft));
    }
    private void DeclineDrawOfferIfPending(string playerId)
    {
        if (PlayerWithDrawOffer.HasValue && GetColorById(playerId) != PlayerWithDrawOffer)
            DeclineDrawOffer(playerId);
    }

    private void SwapColors() 
        => CurrentPlayerColor = CurrentPlayerColor == PlayerColor.White
            ? PlayerColor.Black
            : PlayerColor.White;

    private void EndGameAndPublish(GameResult result, GameResultReason reason)
    {
        AddDomainEvent(new GameEnded(GameId, WhitePlayerId, BlackPlayerId, result, reason));
        IsOver = true;
    }
    
    private bool CheckAndPublishThreefold(MoveResult moveResult)
    {
        if (!moveResult.IsCaptureOrPawnMove!.Value)
        {
            _positionsForThreefoldCount.TryAdd(CurrentFen.PiecesPlacement, 0);
            _positionsForThreefoldCount[CurrentFen.PiecesPlacement]++;
            if (_positionsForThreefoldCount[CurrentFen.PiecesPlacement] == 3)
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
        if (CurrentPlayerColor is PlayerColor.White)
            WhiteTimeLeft += Increment;
        else
            BlackTimeLeft += Increment;
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