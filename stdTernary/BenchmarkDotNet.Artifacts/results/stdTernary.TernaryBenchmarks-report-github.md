```

BenchmarkDotNet v0.14.0, macOS Sonoma 14.5 (23F79) [Darwin 23.5.0]
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                   | Mean           | Error         | StdDev          | Median         | Gen0       | Allocated    |
|------------------------- |---------------:|--------------:|----------------:|---------------:|-----------:|-------------:|
| TestIntTAddition         |       870.6 μs |      17.38 μs |        30.43 μs |       859.0 μs |    16.6016 |    140.63 KB |
| TestIntTSubtraction      |     1,307.8 μs |      24.28 μs |        23.84 μs |     1,309.9 μs |    21.4844 |     187.5 KB |
| TestIntTMultiplication   |    29,994.8 μs |     597.58 μs |     1,077.55 μs |    29,616.0 μs |   437.5000 |   3691.43 KB |
| TestIntTDivision         | 6,451,045.2 μs | 400,232.90 μs | 1,167,498.24 μs | 6,423,793.8 μs | 57000.0000 | 471782.98 KB |
| TestIntTModulus          | 7,713,429.2 μs | 585,930.35 μs | 1,681,144.13 μs | 7,412,070.9 μs | 86000.0000 | 710028.95 KB |
| TestIntTPower            |   143,226.0 μs |   1,142.98 μs |     1,069.14 μs |   143,668.3 μs |  1750.0000 |  15704.39 KB |
| TestFloatTAddition       |    36,747.8 μs |     505.60 μs |       472.94 μs |    36,894.9 μs |  1153.8462 |    9518.4 KB |
| TestFloatTSubtraction    |    30,752.3 μs |   1,084.56 μs |     3,197.84 μs |    29,852.7 μs |  1142.8571 |   9873.72 KB |
| TestFloatTMultiplication |             NA |            NA |              NA |             NA |         NA |           NA |
| TestFloatTDivision       |             NA |            NA |              NA |             NA |         NA |           NA |
| TestFloatTModulus        |     4,295.8 μs |      37.49 μs |        35.07 μs |     4,303.8 μs |   148.4375 |   1231.45 KB |

Benchmarks with issues:
  TernaryBenchmarks.TestFloatTMultiplication: DefaultJob
  TernaryBenchmarks.TestFloatTDivision: DefaultJob
