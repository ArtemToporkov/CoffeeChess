using BenchmarkDotNet.Running;
using CoffeeChess.Benchmark.Benchmarks;

namespace CoffeeChess.Benchmark;

public static class Program
{
    public static void Main()
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run();
    }
}