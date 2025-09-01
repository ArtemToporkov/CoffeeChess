```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.22631.5624/23H2/2023Update/SunValley3)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  Job-CNUJVU : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                | Scenario          | Implementation | Mean        | Error       | StdDev     | Median      | Min         | Max         | Allocated |
|---------------------- |------------------ |--------------- |------------:|------------:|-----------:|------------:|------------:|------------:|----------:|
| **FindMatchingChallenge** | **OnlyLastMatching**  | **LuaScript**      |    **970.7 μs** |    **58.27 μs** |   **170.9 μs** |    **963.2 μs** |    **643.9 μs** |  **1,430.2 μs** |  **20.54 KB** |
| **FindMatchingChallenge** | **OnlyLastMatching**  | **RepositoryScan** | **39,978.0 μs** | **1,975.02 μs** | **5,823.4 μs** | **36,448.8 μs** | **34,398.3 μs** | **52,634.3 μs** | **908.31 KB** |
| **FindMatchingChallenge** | **OnlyFirstMatching** | **LuaScript**      |  **1,016.1 μs** |    **54.10 μs** |   **157.0 μs** |  **1,000.0 μs** |    **694.4 μs** |  **1,352.5 μs** |  **20.52 KB** |
| **FindMatchingChallenge** | **OnlyFirstMatching** | **RepositoryScan** | **34,560.2 μs** |   **605.40 μs** | **1,562.7 μs** | **34,276.7 μs** | **32,013.4 μs** | **42,396.6 μs** | **797.88 KB** |
| **FindMatchingChallenge** | **HalfMatching**      | **LuaScript**      |  **5,322.3 μs** |   **132.32 μs** |   **390.1 μs** |  **5,288.8 μs** |  **4,444.1 μs** |  **6,334.2 μs** |  **36.59 KB** |
| **FindMatchingChallenge** | **HalfMatching**      | **RepositoryScan** |  **1,672.8 μs** |    **96.28 μs** |   **283.9 μs** |  **1,688.2 μs** |  **1,148.6 μs** |  **2,348.3 μs** |  **60.91 KB** |
