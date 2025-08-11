using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Exceptions;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Services.Interfaces;
using MediatR;

namespace CoffeeChess.Application.Games.EventHandlers;

public class GameEndedEventHandler(
    IGameRepository gameRepository,
    ICompletedGameRepository completedGameRepository,
    IRatingService ratingService,
    IPlayerRepository playerRepository,
    IGameEventNotifierService notifier) : INotificationHandler<GameEnded>
{
    public async Task Handle(GameEnded notification, CancellationToken cancellationToken)
    {
        var white = await playerRepository.GetByIdAsync(notification.WhiteId, cancellationToken) 
                    ?? throw new NotFoundException(nameof(Player), notification.WhiteId);
        
        var black = await playerRepository.GetByIdAsync(notification.BlackId, cancellationToken) 
                    ?? throw new NotFoundException(nameof(Player), notification.BlackId);
        
        var (newWhiteRating, newBlackRating) = ratingService.CalculateNewRatings(
            white.Rating, black.Rating,
            notification.GameResult);

        await UpdateRatingAndSave(white.Id, newWhiteRating, cancellationToken);
        await UpdateRatingAndSave(black.Id, newBlackRating, cancellationToken);

        var game = await gameRepository.GetByIdAsync(notification.GameId, cancellationToken) 
                   ?? throw new NotFoundException(nameof(Game), notification.GameId);
        await SaveCompletedGameAsync(
            game, white, black, newWhiteRating, newBlackRating, notification.GameResult, cancellationToken);
        

        var (whiteReason, blackReason) = GetMessageByGameResultReason(notification.GameResult,
            notification.GameResultReason, white.Name, black.Name);
        await notifier.NotifyGameEnded(white, black, notification.GameResult,
            whiteReason, blackReason, cancellationToken);
    }

    private async Task SaveCompletedGameAsync(
        Game game, Player white, Player black, int whiteNewRating, int blackNewRating, GameResult gameResult, 
        CancellationToken cancellationToken = default)
    {
        var completedGame = new CompletedGameReadModel
        {
            GameId = game.GameId,
            
            WhitePlayerId = white.Id,
            WhitePlayerName = white.Name,
            WhitePlayerRating = white.Rating,
            WhitePlayerNewRating = whiteNewRating,
            
            BlackPlayerId = black.Id,
            BlackPlayerName = black.Name,
            BlackPlayerRating = black.Rating,
            BlackPlayerNewRating = blackNewRating,
            
            Minutes = (int)game.InitialTimeForOnePlayer.TotalMinutes,
            Increment = (int)game.Increment.TotalSeconds,
            GameResult = gameResult,
            PlayedDate = game.LastTimeUpdate,
            MovesHistory = game.MovesHistory.ToList()
        };

        await completedGameRepository.AddAsync(completedGame, cancellationToken);
    }

    private async Task UpdateRatingAndSave(string playerId, int newRating,
        CancellationToken cancellationToken = default)
    {
        var player = await playerRepository.GetByIdAsync(playerId, cancellationToken) 
                     ?? throw new NotFoundException(nameof(Player), playerId);
        player.UpdateRating(newRating);
        await playerRepository.SaveChangesAsync(player, cancellationToken);
    }

    private static (string WhiteReason, string BlackReason) GetMessageByGameResultReason(GameResult result,
        GameResultReason reason, string whiteName, string blackName) 
    {
        return reason switch
        {
            GameResultReason.OpponentResigned when result is GameResult.BlackWon 
                => ("you resigned.", $"{whiteName} resigned."),
            GameResultReason.OpponentResigned when result is GameResult.WhiteWon 
                => ($"{blackName} resigned.", "you resigned."),
            GameResultReason.OpponentTimeRanOut when result is GameResult.BlackWon
                => ("your time is run up.", $"{whiteName}'s time is run up."),
            GameResultReason.OpponentTimeRanOut when result is GameResult.WhiteWon
                => ($"{blackName}'s time is run up.", "your time is run up."),
            GameResultReason.Checkmate => ("checkmate.", "checkmate."),
            GameResultReason.Agreement => ("by agreement.", "by agreement."),
            GameResultReason.Stalemate => ("stalemate.", "stalemate."),
            GameResultReason.Threefold => ("by threefold.", "by threefold."),
            GameResultReason.FiftyMovesRule => ("by 50-moves rule.", "by 50-moves rule."),
            _ => throw new ArgumentException(
                $"Invalid game result and its reason combination: {result.ToString()} + {reason.ToString()}")
        };
    }
}