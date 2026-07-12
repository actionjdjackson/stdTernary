```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.5 (25F71) [Darwin 25.5.0]
Apple M4 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.103
  [Host]   : .NET 10.0.3 (10.0.3, 10.0.326.7603), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.3 (10.0.3, 10.0.326.7603), Arm64 RyuJIT armv8.0-a

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                      | DataSize | Mean         | Error        | StdDev      | Gen0     | Gen1    | Allocated |
|---------------------------- |--------- |-------------:|-------------:|------------:|---------:|--------:|----------:|
| **TestTernaryQuickSortLarge**   | **10**       |   **1,372.9 ns** |     **57.19 ns** |     **3.13 ns** |   **0.0114** |       **-** |     **104 B** |
| TestBinaryQuickSortLarge    | 10       |     129.6 ns |     27.90 ns |     1.53 ns |   0.0076 |       - |      64 B |
| TestTernarySearchTreeMemory | 10       |   1,861.8 ns |     76.39 ns |     4.19 ns |   1.2169 |  0.0038 |   10192 B |
| TestBinarySearchTreeMemory  | 10       |     580.7 ns |     13.13 ns |     0.72 ns |   0.2556 |  0.0010 |    2144 B |
| **TestTernaryQuickSortLarge**   | **100**      |  **23,224.6 ns** |    **324.43 ns** |    **17.78 ns** |   **0.0916** |       **-** |     **824 B** |
| TestBinaryQuickSortLarge    | 100      |   2,729.4 ns |    166.65 ns |     9.13 ns |   0.0496 |       - |     424 B |
| TestTernarySearchTreeMemory | 100      |  22,674.1 ns |    487.60 ns |    26.73 ns |  13.8550 |  0.2747 |  116032 B |
| TestBinarySearchTreeMemory  | 100      |   9,173.2 ns |    382.77 ns |    20.98 ns |   2.5787 |  0.0763 |   21616 B |
| **TestTernaryQuickSortLarge**   | **1000**     | **336,051.7 ns** | **11,118.60 ns** |   **609.45 ns** |   **0.4883** |       **-** |    **8024 B** |
| TestBinaryQuickSortLarge    | 1000     |  37,476.0 ns |  1,969.88 ns |   107.98 ns |   0.4272 |       - |    4024 B |
| TestTernarySearchTreeMemory | 1000     | 288,237.4 ns |  4,049.60 ns |   221.97 ns | 168.4570 | 25.3906 | 1412032 B |
| TestBinarySearchTreeMemory  | 1000     | 135,278.7 ns | 19,668.57 ns | 1,078.10 ns |  25.6348 |  6.1035 |  215984 B |
