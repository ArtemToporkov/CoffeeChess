using BenchmarkDotNet.Attributes;
using CoffeeChess.Application.Games.Commands;
using CoffeeChess.Application.Games.Services.Interfaces;
using CoffeeChess.Benchmark.Mocks;
using CoffeeChess.Domain.Games.AggregatesRoots;
using CoffeeChess.Domain.Games.Repositories.Interfaces;
using CoffeeChess.Domain.Games.Services.Implementations;
using CoffeeChess.Domain.Games.Services.Interfaces;
using CoffeeChess.Infrastructure.Repositories.Implementations;
using CoffeeChess.Infrastructure.Services.Implementations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoffeeChess.Benchmark.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class RedisGameRepositoryBenchmark
{
    private IMediator _mediator = null!;
    private IGameRepository _gameRepository = null!;
    private string _gameId = null!;

    [Params("HashesAndList", "Json")]
    public string RepositoryType = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MakeMoveCommand>());
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
        services.AddScoped<IChessMovesValidatorService, ChessDotNetCoreMovesValidatorService>();
        services.AddScoped<IPgnBuilderService, StringBuilderPgnBuilderService>();
        services.AddScoped<IGameEventNotifierService, MockGameEventNotifierService>();

        switch (RepositoryType)
        {
            case "HashesAndList":
                services.AddSingleton<IGameRepository, RedisHashesAndListGameRepository>();
                break;
            case "Json":
                services.AddSingleton<IGameRepository, RedisJsonGameRepository>();
                break;
        }
        
        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
        _gameRepository = serviceProvider.GetRequiredService<IGameRepository>();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _gameId = Guid.NewGuid().ToString("N");
        var game = new Game(
            _gameId,
            "player-white-id",
            "player-black-id",
            TimeSpan.FromMinutes(5),
            TimeSpan.FromSeconds(3)
        );
        _gameRepository.AddAsync(game).GetAwaiter().GetResult();
    }

    [Benchmark]
    public async Task PlayTenMovesInGame()
    {
        var moves = new (string From, string To)[]
        {
            ("e2", "e4"), ("d7", "d5"),
            ("e4", "d5"), ("d8", "d5"),
            ("e1", "e2"), ("h2", "h4")
        };

        for (var i = 0; i < moves.Length; i++)
        {
            var playerId = i % 2 == 0 
                ? "player-white-id" 
                : "player-black-id";
            var move = new MakeMoveCommand(_gameId, playerId, moves[i].From, moves[i].To, null);
            await _mediator.Send(move);
        }
    }
}