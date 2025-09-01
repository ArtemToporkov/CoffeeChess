using System.Text.Json;
using CoffeeChess.Domain.Games.Events;
using Confluent.Kafka;
using MediatR;

namespace CoffeeChess.Consumers.Consumers;

public class GameEndedConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private const string GameEndedEventsTopic = "game-ended-events";

    public GameEndedConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "coffee-chess-game-ended-processor",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            _consumer.Subscribe(GameEndedEventsTopic);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);
                    var gameEndedEvent = JsonSerializer.Deserialize<GameEnded>(result.Message.Value);

                    if (gameEndedEvent is null) 
                        continue;
                    
                    using var scope = _serviceProvider.CreateScope();
                        
                    // TODO: think about getting rid of MediatR dependency
                    var gameEventHandler = scope.ServiceProvider.GetRequiredService<INotificationHandler<GameEnded>>();
                    var chatEventHandler = scope.ServiceProvider.GetRequiredService<INotificationHandler<GameEnded>>();

                    gameEventHandler.Handle(gameEndedEvent, stoppingToken).Wait(stoppingToken);
                    chatEventHandler.Handle(gameEndedEvent, stoppingToken).Wait(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _consumer.Close();
            }
        }, stoppingToken);
    }
}