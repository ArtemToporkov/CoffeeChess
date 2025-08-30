```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.22631.5624/23H2/2023Update/SunValley3)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  Job-CNUJVU : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                | RepositoryType | Mean      | Error     | StdDev    | Min       | Max       | Median    | Gen0      | Allocated  |
|---------------------- |--------------- |----------:|----------:|----------:|----------:|----------:|----------:|----------:|-----------:|
| **PlayTenMovesInGame**    | **HashesAndList**  |  **9.384 ms** | **0.3746 ms** | **1.0566 ms** |  **7.287 ms** | **12.269 ms** |  **9.290 ms** |         **-** |  **729.87 KB** |
| PlayThirtyMovesInGame | HashesAndList  | 39.520 ms | 3.3303 ms | 9.8194 ms | 19.635 ms | 62.922 ms | 41.573 ms | 1000.0000 | 3692.37 KB |
| **PlayTenMovesInGame**    | **InMemory**       |  **1.397 ms** | **0.0841 ms** | **0.2399 ms** |  **1.035 ms** |  **1.985 ms** |  **1.421 ms** |         **-** |  **449.53 KB** |
| PlayThirtyMovesInGame | InMemory       |  5.888 ms | 0.8086 ms | 2.3329 ms |  3.440 ms | 11.802 ms |  5.796 ms | 1000.0000 | 2451.77 KB |
| **PlayTenMovesInGame**    | **Json**           |  **6.223 ms** | **0.2724 ms** | **0.7771 ms** |  **4.862 ms** |  **8.304 ms** |  **6.098 ms** |         **-** |  **598.12 KB** |
| PlayThirtyMovesInGame | Json           | 29.524 ms | 3.0104 ms | 8.8291 ms | 12.010 ms | 52.865 ms | 30.484 ms | 1000.0000 | 3020.36 KB |
