```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.22631.5624/23H2/2023Update/SunValley3)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  Job-CNUJVU : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method             | RepositoryType | Mean      | Error     | StdDev    | Min       | Max       | Median    | Allocated |
|------------------- |--------------- |----------:|----------:|----------:|----------:|----------:|----------:|----------:|
| **PlayTenMovesInGame** | **HashesAndList**  | **14.208 ms** | **0.4323 ms** | **1.2543 ms** | **11.418 ms** | **17.367 ms** | **14.066 ms** | **643.32 KB** |
| **PlayTenMovesInGame** | **Json**           |  **6.550 ms** | **0.2637 ms** | **0.7733 ms** |  **5.201 ms** |  **8.430 ms** |  **6.429 ms** | **598.12 KB** |
