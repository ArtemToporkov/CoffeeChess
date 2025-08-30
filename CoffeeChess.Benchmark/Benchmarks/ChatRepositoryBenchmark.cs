using BenchmarkDotNet.Attributes;
using CoffeeChess.Benchmark.Enums;
using CoffeeChess.Benchmark.Mocks;
using CoffeeChess.Domain.Chats.AggregatesRoots;
using CoffeeChess.Domain.Chats.Repositories.Interfaces;
using CoffeeChess.Infrastructure.Repositories.Implementations;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoffeeChess.Benchmark.Benchmarks;

[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public class ChatRepositoryBenchmark
{
    private IChatRepository _chatRepository = null!;
    private string _chatId = null!;
    private Chat _chat = null!;

    [UsedImplicitly]
    [Params(ChatRepositoryType.RedisList, ChatRepositoryType.RedisJson)]
    public ChatRepositoryType RepositoryType;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
        services.AddScoped<IMediator, MockMediator>();

        switch (RepositoryType)
        {
            case ChatRepositoryType.RedisList:
                services.AddSingleton<IChatRepository, RedisListChatRepository>();
                break;
            case ChatRepositoryType.RedisJson:
                services.AddSingleton<IChatRepository, RedisJsonChatRepository>();
                break;
        }

        var serviceProvider = services.BuildServiceProvider();
        _chatRepository = serviceProvider.GetRequiredService<IChatRepository>();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _chatId = Guid.NewGuid().ToString("N");
        _chat = new Chat(_chatId);
        _chatRepository.AddAsync(_chat).GetAwaiter().GetResult();
    }

    [Benchmark]
    public async Task SendThirtyMessages()
    {
        for (var i = 0; i < 30; i++)
        {
            _chat.AddMessage("benchmark_user", $"Message number {i}");
            await _chatRepository.SaveChangesAsync(_chat);
        }
    }

    [Benchmark]
    public async Task GetById()
    {
        await _chatRepository.GetByIdAsync(_chatId);
    }

    [Benchmark]
    public async Task SaveChanges()
    {
        await _chatRepository.SaveChangesAsync(_chat);
    }

    [Benchmark]
    public async Task Delete()
    {
        await _chatRepository.DeleteAsync(_chat);
    }
}