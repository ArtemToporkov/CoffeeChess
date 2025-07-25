using System.Collections.Concurrent;
using System.Text;
using ChessDotNetCore;
using CoffeeChess.Domain.Enums;
using CoffeeChess.Domain.Events;
using CoffeeChess.Domain.ValueObjects;

namespace CoffeeChess.Domain.Aggregates;

public class Game(
    string gameId, 
    PlayerInfo whitePlayerInfo,
    PlayerInfo blackPlayerInfo, 
    TimeSpan minutesLeftForPlayer,
    TimeSpan increment)
{
    public string GameId { get; } = gameId;
    public PlayerInfo WhitePlayerInfo { get; } = whitePlayerInfo;
    public PlayerInfo BlackPlayerInfo { get; } = blackPlayerInfo;
    public bool IsOver => _chessGame.GameResult != GameResult.OnGoing &&
                          _chessGame.GameResult != GameResult.Check;
    public TimeSpan WhiteTimeLeft { get; private set; } = minutesLeftForPlayer;
    public TimeSpan BlackTimeLeft { get; private set; } = minutesLeftForPlayer;
    public TimeSpan Increment { get; } = increment;
    public DateTime LastTimeUpdate { get; private set; } = DateTime.UtcNow;
    public ConcurrentQueue<ChatMessage> ChatMessages { get; } = new();
    public PlayerColor CurrentPlayerColor => _chessGame.CurrentPlayer == Player.White 
        ? PlayerColor.White 
        : PlayerColor.Black;
    public PlayerColor? PlayerWithDrawOffer { get; private set; }
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    private readonly ChessGame _chessGame = new();
    private readonly Lock _lockObject = new();
    private readonly List<IDomainEvent> _domainEvents = [];
    
    public void ClearDomainEvents() => _domainEvents.Clear();
    
    public void ApplyMove(string playerId, string from, string to, string? promotion)
    {
        var playerColor = GetColorById(playerId);
        var currentPlayerId = CurrentPlayerColor == PlayerColor.White 
            ? WhitePlayerInfo.Id 
            : BlackPlayerInfo.Id;
        var currentPlayerColor = CurrentPlayerColor;
        
        if (playerId != currentPlayerId)
        {
            _domainEvents.Add(new MoveFailed(playerId, "Not your turn."));
            return;
        }

        var promotionChar = promotion?[0];

        var move = new Move(from, to, CurrentPlayerColor == PlayerColor.White 
            ? Player.White : Player.Black, promotionChar);
        if (_chessGame.MakeMove(move, false) is MoveType.Invalid)
        {
            _domainEvents.Add(new MoveFailed(playerId, "Invalid move."));
            return;
        }
        
        if (PlayerWithDrawOffer.HasValue && playerColor != PlayerWithDrawOffer)
            DeclineDrawOffer(playerId);

        if (UpdateTimeAndCheckTimeout(currentPlayerColor))
        {
            _domainEvents.Add(new MoveFailed(playerId, "Time is ran up."));
            Resign(playerId);
            var (result, whiteReason, blackReason) = playerColor == PlayerColor.White
                ? (Result.BlackWon, "your time is run up.", $"{WhitePlayerInfo.Name}'s time is ran up.")
                : (Result.WhiteWon, $"{BlackPlayerInfo.Name}'s time is ran up.", "your time is ran up.");
            _domainEvents.Add(new GameResultUpdated(WhitePlayerInfo, BlackPlayerInfo, result, whiteReason, blackReason));
            return;
        }

        DoIncrement(currentPlayerColor);
        _domainEvents.Add(new MoveMade(WhitePlayerInfo.Id, BlackPlayerInfo.Id, 
            GetPgn(), WhiteTimeLeft, BlackTimeLeft));
        
        if (_chessGame.ThreeFoldRepeatAndThisCanResultInDraw)
        {
            _domainEvents.Add(new GameResultUpdated(WhitePlayerInfo, BlackPlayerInfo, 
                Result.Draw, "by threefold.", "by threefold."));
            return;
        }

        if (_chessGame.FiftyMovesAndThisCanResultInDraw)
        {
            _domainEvents.Add(new GameResultUpdated(WhitePlayerInfo, BlackPlayerInfo, 
                Result.Draw, "by 50-moves rule.", "by 50-moves rule."));
            return;
        }
        
        if (_chessGame.IsCheckmated(Player.White) || _chessGame.IsCheckmated(Player.Black))
        {
            var result = _chessGame.IsCheckmated(Player.White) ? Result.BlackWon : Result.WhiteWon;
            _domainEvents.Add(new GameResultUpdated(WhitePlayerInfo, BlackPlayerInfo, 
                result, "by checkmate.", "by checkmate."));
            return;
        }
        
        if (_chessGame.IsStalemated(Player.White) || _chessGame.IsStalemated(Player.Black))
        { 
            _domainEvents.Add(new GameResultUpdated(WhitePlayerInfo,  BlackPlayerInfo, 
                Result.Draw, "by stalemate.", "by stalemate."));
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
        if (playerId == WhitePlayerInfo.Id)
            return PlayerColor.White;
        if (playerId == BlackPlayerInfo.Id)
            return PlayerColor.Black;
        throw new InvalidOperationException("There's no such player in the game.");
    }
    
    public void OfferADraw(string playerId)
    {
        if (PlayerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's already pending draw offer.");
        PlayerWithDrawOffer = playerId == WhitePlayerInfo.Id ? PlayerColor.White : PlayerColor.Black;
        var senderColor = GetColorById(playerId);
        var (sender, receiver) = senderColor == PlayerColor.White
            ? (WhitePlayerInfo, BlackPlayerInfo) 
            : (BlackPlayerInfo, WhitePlayerInfo);
        _domainEvents.Add(new DrawOfferSent(sender.Name, sender.Id, receiver.Id));
    }

    public void AcceptDrawOffer(string playerId)
    {
        if (!PlayerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (PlayerWithDrawOffer == playerColor)
            throw new InvalidOperationException("The same side tries to offer and accept a draw.");
        PlayerWithDrawOffer = null;
        _domainEvents.Add(new GameResultUpdated(WhitePlayerInfo, BlackPlayerInfo, Result.Draw, 
            "by agreement.", "by agreement."));
    }

    public void DeclineDrawOffer(string playerId)
    {
        if (!PlayerWithDrawOffer.HasValue)
            throw new InvalidOperationException("There's no pending draw offers.");
        var playerColor = GetColorById(playerId);
        if (PlayerWithDrawOffer == playerColor)
            throw new InvalidOperationException("The same side tries to offer and decline a draw.");
        PlayerWithDrawOffer = null;
        var (rejecting, sender) = PlayerWithDrawOffer == PlayerColor.White
            ? (BlackPlayerInfo, WhitePlayerInfo)
            : (WhitePlayerInfo, BlackPlayerInfo);
        _domainEvents.Add(new DrawOfferDeclined(rejecting.Id, sender.Id));
    }

    public void Resign(string playerId) 
    {
        var isWhite = GetColorById(playerId) == PlayerColor.White;
        _chessGame.Resign(isWhite ? Player.White : Player.Black);
        var result = isWhite ? Result.BlackWon : Result.WhiteWon;
        var (whiteReason, blackReason) = isWhite
            ? ("by resignation.", $"{WhitePlayerInfo.Name} resigns.")
            : ($"{BlackPlayerInfo.Name} resigns.", "by resignation.");
        _domainEvents.Add(new GameResultUpdated(WhitePlayerInfo, BlackPlayerInfo, result, whiteReason, blackReason));
    }

    private bool UpdateTimeAndCheckTimeout(PlayerColor playerColor)
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