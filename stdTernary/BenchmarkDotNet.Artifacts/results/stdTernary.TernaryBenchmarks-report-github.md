```

BenchmarkDotNet v0.14.0, macOS Sonoma 14.5 (23F79) [Darwin 23.5.0]
Intel Core i9-9880H CPU 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.402
  [Host]     : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.8 (8.0.824.36612), X64 RyuJIT AVX2


```
| Method                 | Mean          | Error         | StdDev         | Gen0     | Allocated  |
|----------------------- |--------------:|--------------:|---------------:|---------:|-----------:|
| TestIntTAddition       |      8.852 μs |     0.1740 μs |      0.3181 μs |   0.1678 |    1.41 KB |
| TestIntTSubtraction    |     13.472 μs |     0.2591 μs |      0.2880 μs |   0.2136 |    1.88 KB |
| TestIntTMultiplication |            NA |            NA |             NA |       NA |         NA |
| TestIntTDivision       | 44,783.337 μs | 6,976.8186 μs | 19,563.7486 μs | 400.0000 | 3639.33 KB |
| TestIntTModulus        | 48,241.258 μs | 6,383.7833 μs | 17,689.4347 μs | 375.0000 | 3222.86 KB |

Benchmarks with issues:
  TernaryBenchmarks.TestIntTMultiplication: DefaultJob
