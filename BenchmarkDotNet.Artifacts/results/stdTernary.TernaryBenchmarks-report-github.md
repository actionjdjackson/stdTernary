```

BenchmarkDotNet v0.14.0, macOS Sequoia 15.6.1 (24G90) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD


```
| Method                                       | Mean        | Error     | StdDev    | Gen0      | Gen1     | Allocated  |
|--------------------------------------------- |------------:|----------:|----------:|----------:|---------:|-----------:|
| TestTernaryQuickSort                         | 4,383.00 μs | 40.536 μs | 35.934 μs |    7.8125 |        - |    82406 B |
| TestBinaryQuickSort                          |   312.79 μs |  2.088 μs |  1.631 μs |    4.8828 |        - |    42400 B |
| TestTernarySearchTree                        | 4,022.24 μs | 28.377 μs | 26.544 μs | 4421.8750 | 312.5000 | 37013712 B |
| TestBinarySearchTree                         | 3,333.47 μs | 25.494 μs | 23.847 μs |  250.0000 |   3.9063 |  2106403 B |
| TestMethodChainingComparisons                |    31.74 μs |  0.196 μs |  0.163 μs |    2.5635 |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons |    32.31 μs |  0.402 μs |  0.376 μs |    2.8687 |        - |    24000 B |
| TestIfComparisons                            |    30.98 μs |  0.245 μs |  0.204 μs |         - |        - |          - |
| TestCaseComparisons                          |    29.59 μs |  0.326 μs |  0.305 μs |         - |        - |          - |
| TestIntTAddition                             |    74.28 μs |  0.395 μs |  0.350 μs |         - |        - |          - |
| TestIntBAddition                             |    66.30 μs |  0.506 μs |  0.473 μs |         - |        - |          - |
| TestIntTSubtraction                          |    95.11 μs |  0.450 μs |  0.421 μs |         - |        - |          - |
| TestIntBSubtraction                          |    69.67 μs |  0.609 μs |  0.570 μs |         - |        - |          - |
| TestIntTMultiplication                       |   104.54 μs |  0.314 μs |  0.279 μs |         - |        - |          - |
| TestIntBMultiplication                       |   198.69 μs |  0.606 μs |  0.506 μs |         - |        - |          - |
| TestIntTDivision                             |   688.75 μs |  5.865 μs |  5.199 μs |         - |        - |       21 B |
| TestIntBDivision                             |   432.14 μs |  1.604 μs |  1.501 μs |         - |        - |       12 B |
| TestIntTModulus                              |   690.35 μs |  5.097 μs |  4.768 μs |         - |        - |       15 B |
| TestIntBModulus                              |   439.00 μs |  2.078 μs |  1.842 μs |         - |        - |       12 B |
| TestIntTPower                                |    83.27 μs |  0.403 μs |  0.377 μs |         - |        - |          - |
