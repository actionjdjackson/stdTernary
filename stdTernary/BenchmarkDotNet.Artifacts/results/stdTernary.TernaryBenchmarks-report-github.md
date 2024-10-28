```

BenchmarkDotNet v0.14.0, macOS Sonoma 14.5 (23F79) [Darwin 23.5.0]
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                   | Mean            | Error           | StdDev          | Gen0        | Gen1      | Allocated    |
|------------------------- |----------------:|----------------:|----------------:|------------:|----------:|-------------:|
| TestIntTAddition         |        203.8 μs |         3.96 μs |         5.01 μs |      8.5449 |         - |     70.31 KB |
| TestIntTSubtraction      |        218.0 μs |         4.20 μs |         4.13 μs |     11.4746 |         - |     93.75 KB |
| TestIntTMultiplication   |      1,818.8 μs |        32.04 μs |        34.28 μs |    216.7969 |         - |   1781.25 KB |
| TestIntTDivision         |    438,071.5 μs |    35,025.34 μs |   102,723.28 μs |  35000.0000 |         - | 291309.86 KB |
| TestIntTModulus          |    422,855.5 μs |    27,560.72 μs |    80,830.83 μs |  46500.0000 |         - | 380739.43 KB |
| TestIntTPower            |      8,487.1 μs |       145.03 μs |       128.57 μs |    953.1250 |         - |   7861.48 KB |
| TestFloatTAddition       |     11,165.4 μs |       222.62 μs |       312.08 μs |    562.5000 |         - |   4657.75 KB |
| TestFloatTSubtraction    |     10,916.6 μs |       207.33 μs |       203.62 μs |    562.5000 |         - |   4718.79 KB |
| TestFloatTMultiplication |              NA |              NA |              NA |          NA |        NA |           NA |
| TestFloatTDivision       |              NA |              NA |              NA |          NA |        NA |           NA |
| TestFloatTModulus        | 15,540,848.0 μs | 1,885,404.20 μs | 5,224,446.55 μs | 831000.0000 | 4000.0000 | 6790614.8 KB |

Benchmarks with issues:
  TernaryBenchmarks.TestFloatTMultiplication: DefaultJob
  TernaryBenchmarks.TestFloatTDivision: DefaultJob
