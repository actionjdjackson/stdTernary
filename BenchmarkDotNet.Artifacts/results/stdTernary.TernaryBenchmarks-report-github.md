```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.6.1 (24G90) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                                       | Mean        | Error     | StdDev    | Gen0      | Gen1     | Allocated  |
|--------------------------------------------- |------------:|----------:|----------:|----------:|---------:|-----------:|
| TestTernaryQuickSort                         | 4,258.90 μs | 32.396 μs | 30.303 μs |    7.8125 |        - |    82400 B |
| TestBinaryQuickSort                          |   307.48 μs |  2.827 μs |  2.644 μs |    4.8828 |        - |    42400 B |
| TestTernarySearchTree                        | 2,474.72 μs | 19.226 μs | 17.984 μs | 1542.9688 | 113.2813 | 12929080 B |
| TestBinarySearchTree                         | 3,076.61 μs | 23.904 μs | 22.360 μs |  250.0000 |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                |    24.46 μs |  0.357 μs |  0.334 μs |    2.5635 |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons |    24.67 μs |  0.233 μs |  0.218 μs |    2.8687 |        - |    24000 B |
| TestIfComparisons                            |    23.24 μs |  0.298 μs |  0.279 μs |         - |        - |          - |
| TestCaseComparisons                          |    21.86 μs |  0.196 μs |  0.183 μs |         - |        - |          - |
| TestIntTAddition                             |    69.89 μs |  0.350 μs |  0.310 μs |         - |        - |          - |
| TestIntBAddition                             |    66.91 μs |  0.431 μs |  0.403 μs |         - |        - |          - |
| TestIntTSubtraction                          |    87.68 μs |  0.748 μs |  0.699 μs |         - |        - |          - |
| TestIntBSubtraction                          |    70.94 μs |  0.612 μs |  0.573 μs |         - |        - |          - |
| TestIntTMultiplication                       |   100.28 μs |  0.693 μs |  0.648 μs |         - |        - |          - |
| TestIntBMultiplication                       |   175.34 μs |  1.523 μs |  1.425 μs |         - |        - |          - |
| TestIntTDivision                             |   568.80 μs |  6.223 μs |  5.821 μs |         - |        - |       15 B |
| TestIntBDivision                             |   441.63 μs |  4.109 μs |  3.844 μs |         - |        - |       10 B |
| TestIntTModulus                              |   569.80 μs |  4.126 μs |  3.657 μs |         - |        - |       16 B |
| TestIntBModulus                              |   445.37 μs |  2.812 μs |  2.631 μs |         - |        - |       11 B |
| TestIntTPower                                |    68.60 μs |  0.611 μs |  0.572 μs |         - |        - |          - |
