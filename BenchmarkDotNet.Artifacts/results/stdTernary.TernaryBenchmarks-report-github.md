```

BenchmarkDotNet v0.14.0, macOS Sequoia 15.6.1 (24G90) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD


```
| Method                        | Mean      | Error    | StdDev   | Gen0   | Allocated |
|------------------------------ |----------:|---------:|---------:|-------:|----------:|
| TestMethodChainingComparisons |  32.05 μs | 0.265 μs | 0.235 μs | 2.5635 |   21600 B |
| TestIfComparisons             |  30.70 μs | 0.360 μs | 0.319 μs |      - |         - |
| TestCaseComparisons           |  29.83 μs | 0.323 μs | 0.270 μs |      - |         - |
| TestIntTAddition              |  74.60 μs | 0.264 μs | 0.247 μs |      - |         - |
| TestIntTSubtraction           |  95.79 μs | 0.315 μs | 0.295 μs |      - |         - |
| TestIntTMultiplication        | 105.41 μs | 0.359 μs | 0.318 μs |      - |         - |
| TestIntTDivision              | 690.37 μs | 3.345 μs | 3.129 μs |      - |      18 B |
| TestIntTModulus               | 688.24 μs | 2.093 μs | 1.855 μs |      - |      19 B |
| TestIntTPower                 |  82.42 μs | 0.645 μs | 0.603 μs |      - |         - |
| TestFloatTAddition            | 167.34 μs | 0.717 μs | 0.671 μs |      - |         - |
| TestFloatTSubtraction         | 165.81 μs | 0.798 μs | 0.746 μs |      - |         - |
| TestFloatTMultiplication      | 186.42 μs | 0.852 μs | 0.755 μs |      - |         - |
| TestFloatTDivision            | 647.39 μs | 3.701 μs | 3.462 μs |      - |     173 B |
| TestFloatTModulus             | 125.84 μs | 0.756 μs | 0.707 μs |      - |         - |
