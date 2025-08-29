using BenchmarkDotNet.Running;
using CoffeeChess.Benchmark.Benchmarks;

namespace CoffeeChess.Benchmark;

public static class Program
{
    public static void Main()
    {
        BenchmarkRunner.Run<RedisGameRepositoryBenchmark>();
    }
}