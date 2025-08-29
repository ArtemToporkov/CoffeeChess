```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.22631.5624/23H2/2023Update/SunValley3)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  Job-CNUJVU : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method             | RepositoryType | Mean      | Error     | StdDev    | Min      | Max       | Median    | Allocated |
|------------------- |--------------- |----------:|----------:|----------:|---------:|----------:|----------:|----------:|
| **PlayTenMovesInGame** | **HashesAndList**  | **12.384 ms** | **0.4154 ms** | **1.1984 ms** | **8.576 ms** | **15.460 ms** | **12.374 ms** | **723.44 KB** |
| **PlayTenMovesInGame** | **Json**           |  **6.530 ms** | **0.2392 ms** | **0.6901 ms** | **4.495 ms** |  **7.988 ms** |  **6.557 ms** | **606.14 KB** |
