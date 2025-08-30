```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.22631.5624/23H2/2023Update/SunValley3)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  Job-CNUJVU : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method             | RepositoryType | Mean        | Error     | StdDev      | Median      | Min         | Max         | Allocated  |
|------------------- |--------------- |------------:|----------:|------------:|------------:|------------:|------------:|-----------:|
| **SendThirtyMessages** | **RedisList**      | **11,106.9 μs** | **388.21 μs** | **1,132.43 μs** | **11,120.4 μs** |  **8,756.4 μs** | **13,620.6 μs** |  **332.92 KB** |
| GetById            | RedisList      |    681.0 μs |  41.66 μs |   117.50 μs |    658.1 μs |    450.9 μs |    993.6 μs |    19.3 KB |
| SaveChanges        | RedisList      |    386.6 μs |  27.57 μs |    78.66 μs |    376.8 μs |    250.6 μs |    580.3 μs |    8.96 KB |
| Delete             | RedisList      |    505.1 μs |  36.10 μs |   105.30 μs |    484.1 μs |    340.7 μs |    778.4 μs |   23.13 KB |
| **SendThirtyMessages** | **RedisJson**      | **20,204.7 μs** | **470.19 μs** | **1,386.36 μs** | **19,895.4 μs** | **18,156.9 μs** | **23,899.6 μs** | **1061.97 KB** |
| GetById            | RedisJson      |    913.1 μs |  64.66 μs |   186.55 μs |    862.0 μs |    677.6 μs |  1,461.8 μs |   51.59 KB |
| SaveChanges        | RedisJson      |    895.0 μs |  50.46 μs |   144.77 μs |    850.5 μs |    672.2 μs |  1,285.4 μs |   66.14 KB |
| Delete             | RedisJson      |    406.6 μs |  22.70 μs |    64.02 μs |    406.4 μs |    270.1 μs |    578.8 μs |    8.62 KB |
