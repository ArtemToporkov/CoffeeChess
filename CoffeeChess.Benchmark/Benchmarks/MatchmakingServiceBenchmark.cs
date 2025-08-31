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
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
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
            EloRatingPreference.Any);
        _noiseChallengeSettings = new ChallengeSettings(
            new TimeControl(3, 2),
            ColorPreference.Any, 
            EloRatingPreference.Any);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _matchmakingService = _serviceProvider.GetRequiredService<IMatchmakingService>();
        var noiseChallengesWithMatchingInTheMiddle = new List<Challenge>();
        for (var i = 0; i < NoiseChallengesCount; i++)
            noiseChallengesWithMatchingInTheMiddle.Add(new($"player_{i}", _noiseRating, _noiseChallengeSettings));
        noiseChallengesWithMatchingInTheMiddle[NoiseChallengesCount / 2] = new(
            $"player_{NoiseChallengesCount / 2}", _ratingToMatch, _challengeSettingsToMatch);
        
        switch (Implementation)
        {
            case MatchmakingServiceType.LuaScript:
                noiseChallengesWithMatchingInTheMiddle.ForEach(async c => await _matchmakingService.AddAsync(c));
                break;
            case MatchmakingServiceType.RepositoryScan:
                var challengeRepository = _serviceProvider.GetRequiredService<IChallengeRepository>();
                noiseChallengesWithMatchingInTheMiddle.ForEach(async c => await challengeRepository.AddAsync(c));
                break;
        }
    }

    [Benchmark]
    public async Task FindMatch()
    {
        const string playerId = "some_player_that_wants_to_find_challenge";
        var found = await _matchmakingService.QueueOrFindMatchingChallenge(
            playerId, _ratingToMatch, _challengeSettingsToMatch);
        if (!found)
            throw new Exception("Matching challenge is not found.");
    }
}