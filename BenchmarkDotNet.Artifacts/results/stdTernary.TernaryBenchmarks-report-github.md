```

BenchmarkDotNet v0.14.0, macOS Sequoia 15.6.1 (24G90) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD


```
| Method                | Mean       | Error    | StdDev   | Gen0      | Gen1     | Allocated   |
|---------------------- |-----------:|---------:|---------:|----------:|---------:|------------:|
| TestTernaryQuickSort  | 4,452.0 μs | 32.43 μs | 28.75 μs |    7.8125 |        - |    80.47 KB |
| TestBinaryQuickSort   |   314.8 μs |  1.13 μs |  1.00 μs |    4.8828 |        - |    41.41 KB |
| TestTernarySearchTree | 4,097.0 μs | 60.64 μs | 53.76 μs | 4421.8750 | 312.5000 | 36147.92 KB |
| TestBinarySearchTree  | 3,307.3 μs | 25.59 μs | 23.94 μs |  250.0000 |   3.9063 |  2057.03 KB |
