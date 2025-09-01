using BenchmarkDotNet.Attributes;
using CoffeeChess.Application.Matchmaking.Services.Interfaces;
using CoffeeChess.Benchmark.Enums;
using CoffeeChess.Infrastructure.Repositories.Implementations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using CoffeeChess.Benchmark.Mocks;
using CoffeeChess.Domain.Matchmaking.Entities;
using CoffeeChess.Domain.Matchmaking.Enums;
using CoffeeChess.Domain.Matchmaking.Repositories.Interfaces;
using CoffeeChess.Domain.Matchmaking.ValueObjects;
using CoffeeChess.Infrastructure.Services.Implementations;
using JetBrains.Annotations;

namespace CoffeeChess.Benchmark.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class MatchmakingBenchmark
{
    private const int NoiseChallengesCount = 1000;
    private static ServiceProvider _serviceProvider = null!;
    private static IMatchmakingService _matchmakingService = null!;
    private static ChallengeSettings _challengeSettingsToMatch;
    private static ChallengeSettings _noiseChallengeSettings;
    private static int _ratingToMatch;
    private static int _noiseRating;


    [UsedImplicitly]
    [Params(MatchmakingServiceType.LuaScript, MatchmakingServiceType.RepositoryScan)]
    public MatchmakingServiceType Implementation;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        // TODO: use other redis server
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
            "localhost:6379,allowAdmin=true"));
        services.AddScoped(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
        services.AddSingleton<IMediator, MockMediator>();

        switch (Implementation)
        {
            case MatchmakingServiceType.LuaScript:
                services.AddScoped<IMatchmakingService, RedisLuaScriptMatchmakingService>();
                break;
            case MatchmakingServiceType.RepositoryScan:
                services.AddScoped<IChallengeRepository, RedisHashesChallengeRepository>();
                services.AddScoped<IMatchmakingService, RedisRepositoryScanMatchmakingService>();
                break;
        }

        _serviceProvider = services.BuildServiceProvider();
        _ratingToMatch = 1500;
        _noiseRating = 0;
        _challengeSettingsToMatch = new ChallengeSettings(
            new TimeControl(3, 2),
            ColorPreference.Any,
            new EloRatingPreference(_ratingToMatch - 100, _ratingToMatch + 100));
        _noiseChallengeSettings = new ChallengeSettings(
            new TimeControl(1, 1),
            ColorPreference.Any, 
            EloRatingPreference.Any);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _matchmakingService = _serviceProvider.GetRequiredService<IMatchmakingService>();
        var noiseChallengesWithMatchingInTheMiddle = new List<Challenge>();
        for (var i = 0; i < NoiseChallengesCount; i++)
            noiseChallengesWithMatchingInTheMiddle.Add(
                new($"player_{i}", _noiseRating, _noiseChallengeSettings));
        
        const int middle = NoiseChallengesCount / 2;
        noiseChallengesWithMatchingInTheMiddle[middle] = new(
            $"player_{middle}", _ratingToMatch, _challengeSettingsToMatch);
        
        switch (Implementation)
        {
            case MatchmakingServiceType.LuaScript:
                var luaTasks = noiseChallengesWithMatchingInTheMiddle
                    .Select(c => _matchmakingService.AddAsync(c))
                    .ToArray();
                Task.WhenAll(luaTasks).Wait();
                break;
            case MatchmakingServiceType.RepositoryScan:
                var challengeRepository = _serviceProvider.GetRequiredService<IChallengeRepository>();
                var repoTasks = noiseChallengesWithMatchingInTheMiddle
                    .Select(c => challengeRepository.AddAsync(c))
                    .ToArray();
                Task.WhenAll(repoTasks).Wait();
                break;
        }
    }

    [IterationCleanup]
    public static void IterationCleanup()
    {
        var multiplexer = _serviceProvider.GetRequiredService<IConnectionMultiplexer>();
        var server = multiplexer.GetServer(multiplexer.GetEndPoints().First());
        server.FlushDatabase();
    }

    [Benchmark]
#pragma warning disable CA1822
    public async Task FindMatchingChallenge_InTheMiddleOfThousandChallenges()
#pragma warning restore CA1822
    {
        const string playerId = "some_player_that_wants_to_find_challenge";
        var found = await _matchmakingService.QueueOrFindMatchingChallenge(
            playerId, _ratingToMatch, _challengeSettingsToMatch);
        if (!found)
            throw new Exception("Matching challenge is not found.");
    }
    
    [Benchmark]
#pragma warning disable CA1822
    public async Task FindMatchingChallenge_First()
#pragma warning restore CA1822
    {
        const string playerId = "some_player_that_wants_to_find_challenge";
        var found = await _matchmakingService.QueueOrFindMatchingChallenge(
            playerId, _noiseRating, _noiseChallengeSettings);
        if (!found)
            throw new Exception("Matching challenge is not found.");
    }
}