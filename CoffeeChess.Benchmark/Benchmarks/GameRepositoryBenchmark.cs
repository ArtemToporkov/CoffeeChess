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
public class GameRepositoryBenchmark
{
    private IMediator _mediator = null!;
    private IGameRepository _gameRepository = null!;
    private string _gameId = null!;
    private Game _game = null!;

    [Params("HashesAndList", "Json", "InMemory")]
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
            case "InMemory":
                services.AddSingleton<IGameRepository, InMemoryGameRepository>();
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
        _game = new Game(
            _gameId,
            "player-white-id",
            "player-black-id",
            TimeSpan.FromMinutes(5),
            TimeSpan.FromSeconds(3)
        );
        _gameRepository.AddAsync(_game).GetAwaiter().GetResult();
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
    
    [Benchmark]
    public async Task PlayThirtyMovesInGame()
    {
        var moves = new (string From, string To)[]
        {
            ("a2", "a3"), ("a7", "a6"),
            ("b2", "b3"), ("b7", "b6"),
            ("c2", "c3"), ("c7", "c6"),
            ("d2", "d3"), ("d7", "d6"),
            ("e2", "e3"), ("e7", "e6"),
            ("f2", "f3"), ("f7", "f6"),
            ("g2", "g3"), ("g7", "g6"),
            ("h2", "h3"), ("h7", "h6"),
            ("a3", "a4"), ("a6", "a5"),
            ("b3", "b4"), ("b6", "b5"),
            ("c3", "c4"), ("c6", "c5"),
            ("d3", "d4"), ("d6", "d5"),
            ("e3", "e4"), ("e6", "e5"),
            ("f3", "f4"), ("f6", "f5"),
            ("g3", "g4"), ("g6", "g5"),
            ("h3", "h4"), ("h6", "h5")
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
    
    [Benchmark]
    public async Task GetById()
    {
        await _gameRepository.GetByIdAsync(_gameId, CancellationToken.None);
    }
    
    [Benchmark]
    public async Task SaveChanges()
    {
        await _gameRepository.SaveChangesAsync(_game, CancellationToken.None);
    }
    
    [Benchmark]
    public async Task Delete()
    {
        await _gameRepository.DeleteAsync(_game, CancellationToken.None);
    }
}