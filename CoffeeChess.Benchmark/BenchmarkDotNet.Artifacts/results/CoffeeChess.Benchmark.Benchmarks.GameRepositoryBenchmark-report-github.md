```

BenchmarkDotNet v0.15.2, Windows 11 (10.0.22631.5624/23H2/2023Update/SunValley3)
AMD Ryzen 5 5500U with Radeon Graphics 2.10GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK 9.0.300
  [Host]     : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2
  Job-CNUJVU : .NET 9.0.5 (9.0.525.21509), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                | RepositoryType | Mean          | Error         | StdDev         | Median        | Min            | Max           | Gen0      | Allocated |
|---------------------- |--------------- |--------------:|--------------:|---------------:|--------------:|---------------:|--------------:|----------:|----------:|
| **PlayTenMovesInGame**    | **HashesAndList**  | **10,470.894 μs** |   **531.3899 μs** |  **1,481.3027 μs** | **10,081.900 μs** |  **7,628.2500 μs** | **14,559.450 μs** |         **-** |  **804896 B** |
| PlayThirtyMovesInGame | HashesAndList  | 43,646.331 μs | 4,335.9859 μs | 12,648.2754 μs | 45,653.100 μs | 23,140.6000 μs | 76,493.600 μs | 1000.0000 | 3772224 B |
| GetById               | HashesAndList  |    824.000 μs |    50.5059 μs |    148.1251 μs |    766.900 μs |    572.2000 μs |  1,153.700 μs |         - |   34176 B |
| SaveChanges           | HashesAndList  |    617.785 μs |    29.9783 μs |     85.5296 μs |    616.700 μs |    424.8000 μs |    865.500 μs |         - |   30936 B |
| Delete                | HashesAndList  |    516.197 μs |    28.3583 μs |     79.0515 μs |    508.600 μs |    359.9000 μs |    709.500 μs |         - |   24472 B |
| **PlayTenMovesInGame**    | **InMemory**       |  **1,167.657 μs** |    **28.7862 μs** |     **77.3323 μs** |  **1,175.800 μs** |  **1,048.8000 μs** |  **1,397.000 μs** |         **-** |  **459744 B** |
| PlayThirtyMovesInGame | InMemory       |  5,855.894 μs |   859.6647 μs |  2,534.7411 μs |  4,272.600 μs |  3,461.3000 μs | 11,400.400 μs | 1000.0000 | 2510616 B |
| GetById               | InMemory       |      6.690 μs |     1.0369 μs |      3.0248 μs |      5.550 μs |      2.5500 μs |     15.850 μs |         - |      72 B |
| SaveChanges           | InMemory       |      1.541 μs |     0.2838 μs |      0.8143 μs |      1.300 μs |      0.5000 μs |      3.700 μs |         - |         - |
| Delete                | InMemory       |      3.883 μs |     0.6531 μs |      1.8843 μs |      3.300 μs |      1.2000 μs |      8.500 μs |         - |         - |
| **PlayTenMovesInGame**    | **Json**           |  **6,503.331 μs** |   **223.8710 μs** |    **653.0424 μs** |  **6,468.000 μs** |  **4,852.3500 μs** |  **8,054.250 μs** |         **-** |  **628904 B** |
| PlayThirtyMovesInGame | Json           | 31,122.716 μs | 2,511.6586 μs |  7,405.6825 μs | 32,709.100 μs | 14,759.5000 μs | 43,757.400 μs | 1000.0000 | 3092872 B |
| GetById               | Json           |    580.594 μs |    34.9443 μs |    100.2617 μs |    559.100 μs |    402.0000 μs |    827.200 μs |         - |   15456 B |
| SaveChanges           | Json           |    512.851 μs |    26.9985 μs |     76.1497 μs |    508.100 μs |    370.9500 μs |    708.350 μs |         - |   27288 B |
| Delete                | Json           |    441.160 μs |    21.3202 μs |     60.8278 μs |    438.150 μs |    318.4000 μs |    610.900 μs |         - |    8824 B |
