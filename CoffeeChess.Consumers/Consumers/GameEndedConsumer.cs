using System.Runtime.Serialization;
using System.Text.Json;
using CoffeeChess.Application.Games.ReadModels;
using CoffeeChess.Application.Games.Repositories.Interfaces;
using CoffeeChess.Application.Shared.Exceptions;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Enums;
using CoffeeChess.Domain.Games.Events;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Players.AggregatesRoots;
using CoffeeChess.Domain.Players.Repositories.Interfaces;
using CoffeeChess.Domain.Players.Services.Interfaces;
using CoffeeChess.Infrastructure.Exceptions;
using Confluent.Kafka;

namespace CoffeeChess.Consumers.Consumers;

public class GameEndedConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly string _gameEndedEventsTopic;

    public GameEndedConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new KafkaConfigurationException(
                $"Cannot find a parameter for \"BootstrapServers\" in {nameof(configuration)}."),
            GroupId = configuration["Kafka:GameEndedConsumerGroup"] ?? throw new KafkaConfigurationException(
                $"Cannot find a parameter for \"GameEndedConsumerGroup\" in {nameof(configuration)}."),
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _gameEndedEventsTopic = configuration["Kafka:GameEndedEventsTopic"] ?? throw new KafkaConfigurationException(
            $"Cannot find a name for \"{nameof(GameEnded)}\" events in {nameof(configuration)}.");
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            _consumer.Subscribe(_gameEndedEventsTopic);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);
                    var gameEndedEvent = JsonSerializer.Deserialize<GameEnded>(result.Message.Value);

                    if (gameEndedEvent is null)
                        throw new SerializationException(
                            $"{nameof(GameEnded)} with value \"{result.Message.Value}\" " +
                            $"was not deserialized successfully.");
                    
                    using var scope = _serviceProvider.CreateScope();
                    var playerRepository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
                    var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
                    var completedGameRepository = scope.ServiceProvider.GetRequiredService<ICompletedGameRepository>();
                    var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
                    
                    Handle(gameEndedEvent, 
                        playerRepository, 
                        gameRepository, 
                        completedGameRepository, 
                        ratingService, 
                        stoppingToken).Wait(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _consumer.Close();
            }
        }, stoppingToken);
    }

    private static async Task Handle(
        GameEnded notification, 
        IPlayerRepository playerRepository,
        IGameRepository gameRepository, 
        ICompletedGameRepository completedGameRepository,
        IRatingService ratingService, 
        CancellationToken cancellationToken = default)
    {
        var white = await playerRepository.GetByIdAsync(notification.WhiteId, cancellationToken) 
                    ?? throw new NotFoundException(nameof(Player), notification.WhiteId);
        
        var black = await playerRepository.GetByIdAsync(notification.BlackId, cancellationToken) 
                    ?? throw new NotFoundException(nameof(Player), notification.BlackId);

        var (whiteRating, blackRating) = (white.Rating, black.Rating);
        var (newWhiteRating, newBlackRating) = ratingService.CalculateNewRatings(
            white.Rating, black.Rating,
            notification.GameResult);

        await UpdateRatingAndSaveAsync(playerRepository, white.Id, newWhiteRating, cancellationToken);
        await UpdateRatingAndSaveAsync(playerRepository, black.Id, newBlackRating, cancellationToken);

        var game = await gameRepository.GetByIdAsync(notification.GameId, cancellationToken) 
                   ?? throw new NotFoundException(nameof(Game), notification.GameId);
        await SaveCompletedGameAsync(
            completedGameRepository,
            game, 
            white, black, 
            whiteRating, newWhiteRating, 
            blackRating, newBlackRating, 
            notification.GameResult, notification.GameResultReason,
            cancellationToken);
    }

    private static async Task SaveCompletedGameAsync(
        ICompletedGameRepository completedGameRepository,
        Game game, 
        Player white, Player black, 
        int whiteRating, int whiteNewRating, 
        int blackRating, int blackNewRating, 
        GameResult gameResult, GameResultReason gameResultReason,
        CancellationToken cancellationToken = default)
    {
        var completedGame = new CompletedGameReadModel
        {
            GameId = game.GameId,
            
            WhitePlayerId = white.Id,
            WhitePlayerName = white.Name,
            WhitePlayerRating = whiteRating,
            WhitePlayerNewRating = whiteNewRating,
            
            BlackPlayerId = black.Id,
            BlackPlayerName = black.Name,
            BlackPlayerRating = blackRating,
            BlackPlayerNewRating = blackNewRating,
            
            Minutes = (int)game.InitialTimeForOnePlayer.TotalMinutes,
            Increment = (int)game.Increment.TotalSeconds,
            GameResult = gameResult,
            GameResultReason = gameResultReason,
            PlayedDate = game.LastTimeUpdate,
            MovesHistory = game.MovesHistory.ToList()
        };

        await completedGameRepository.AddAsync(completedGame, cancellationToken);
    }

    private static async Task UpdateRatingAndSaveAsync(
        IPlayerRepository playerRepository, 
        string playerId, 
        int newRating,
        CancellationToken cancellationToken = default)
    {
        var player = await playerRepository.GetByIdAsync(playerId, cancellationToken) 
                     ?? throw new NotFoundException(nameof(Player), playerId);
        player.UpdateRating(newRating);
        await playerRepository.SaveChangesAsync(player, cancellationToken);
    }
}