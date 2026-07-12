```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.5 (25F71) [Darwin 25.5.0]
Apple M4 Max, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.103
  [Host]   : .NET 10.0.3 (10.0.3, 10.0.326.7603), Arm64 RyuJIT armv8.0-a
  ShortRun : .NET 10.0.3 (10.0.3, 10.0.326.7603), Arm64 RyuJIT armv8.0-a

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  

```
| Method                                          | Mean           | Error         | StdDev       | Gen0      | Gen1     | Allocated  |
|------------------------------------------------ |---------------:|--------------:|-------------:|----------:|---------:|-----------:|
| TestTernaryQuickSort                            | 2,152,226.2 ns | 162,361.55 ns |  8,899.58 ns |    7.8125 |        - |    82400 B |
| TestBinaryQuickSort                             |   266,593.1 ns |  54,417.05 ns |  2,982.78 ns |    4.8828 |        - |    42400 B |
| TestTernarySearchTree                           | 2,134,397.7 ns | 143,457.74 ns |  7,863.40 ns | 1542.9688 | 113.2813 | 12927350 B |
| TestBinarySearchTree                            | 2,729,775.0 ns | 371,249.95 ns | 20,349.46 ns |  250.0000 |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                   |    17,798.8 ns |     510.74 ns |     28.00 ns |    2.5635 |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons    |    18,305.8 ns |   1,012.95 ns |     55.52 ns |    2.8687 |        - |    24000 B |
| TestIfComparisons                               |    16,909.8 ns |   3,707.11 ns |    203.20 ns |         - |        - |          - |
| TestCaseComparisons                             |    16,617.1 ns |   3,154.12 ns |    172.89 ns |         - |        - |          - |
| TestIntTAddition                                |    20,723.8 ns |     539.70 ns |     29.58 ns |         - |        - |          - |
| TestIntBAddition                                |    26,063.9 ns |     836.75 ns |     45.87 ns |         - |        - |          - |
| TestUIntTAddition                               |    13,356.7 ns |     860.61 ns |     47.17 ns |         - |        - |          - |
| TestNativeLongAddition                          |       454.8 ns |       7.94 ns |      0.44 ns |         - |        - |          - |
| TestNativeIntAddition                           |       452.3 ns |      19.41 ns |      1.06 ns |         - |        - |          - |
| TestNativeLongMultiplication                    |       464.6 ns |      32.73 ns |      1.79 ns |         - |        - |          - |
| TestIntTSubtraction                             |    25,450.8 ns |   2,739.42 ns |    150.16 ns |         - |        - |          - |
| TestIntBSubtraction                             |    28,085.3 ns |   2,382.49 ns |    130.59 ns |         - |        - |          - |
| TestUIntTSubtraction                            |    18,757.7 ns |   1,579.77 ns |     86.59 ns |         - |        - |          - |
| TestIntTMultiplication                          |    50,046.8 ns |   3,682.60 ns |    201.86 ns |         - |        - |          - |
| TestIntBMultiplication                          |    62,317.8 ns |   4,631.92 ns |    253.89 ns |         - |        - |          - |
| TestUIntTMultiplication                         |    37,928.7 ns |     581.82 ns |     31.89 ns |         - |        - |          - |
| TestIntTDivision                                |   278,690.3 ns |  12,970.47 ns |    710.96 ns |         - |        - |       18 B |
| TestIntBDivision                                |   202,844.4 ns |   5,501.57 ns |    301.56 ns |         - |        - |       13 B |
| TestUIntTDivision                               |   238,637.6 ns |  36,313.13 ns |  1,990.44 ns |         - |        - |          - |
| TestIntTModulus                                 |   279,238.1 ns |   7,441.72 ns |    407.91 ns |         - |        - |       17 B |
| TestIntBModulus                                 |   203,368.3 ns |  15,829.32 ns |    867.66 ns |         - |        - |       11 B |
| TestUIntTModulus                                |   245,544.3 ns |  17,411.29 ns |    954.37 ns |         - |        - |          - |
| TestIntTPower                                   |    47,596.8 ns |   7,354.81 ns |    403.14 ns |         - |        - |          - |
| TestUIntTBitwise                                |    30,344.6 ns |   4,842.02 ns |    265.41 ns |         - |        - |          - |
| TestTryteArithmetic                             |    12,005.0 ns |     486.66 ns |     26.68 ns |         - |        - |          - |
| TestUTryteArithmetic                            |    44,355.3 ns |  16,671.46 ns |    913.82 ns |         - |        - |          - |
| TestTryteBitwise                                |    17,098.4 ns |     708.30 ns |     38.82 ns |         - |        - |          - |
| TestUTryteBitwise                               |     8,670.5 ns |     956.20 ns |     52.41 ns |         - |        - |          - |
| TestTryteVsByteConversion                       |     2,957.3 ns |      42.56 ns |      2.33 ns |         - |        - |          - |
| TestUTryteVsByteConversion                      |     1,546.9 ns |     102.69 ns |      5.63 ns |         - |        - |          - |
| TestTritOperations                              |       460.5 ns |      10.39 ns |      0.57 ns |         - |        - |          - |
| TestUTritOperations                             |       978.0 ns |      41.76 ns |      2.29 ns |         - |        - |          - |
| TestTritDecisionMaking                          |       813.3 ns |     114.86 ns |      6.30 ns |         - |        - |          - |
| TestMixedBalancedUnbalancedSpaceshipComparisons |    14,543.0 ns |   1,202.44 ns |     65.91 ns |         - |        - |          - |
| TestFloatTAddition                              |    54,930.2 ns |     625.28 ns |     34.27 ns |         - |        - |          - |
| TestUFloatTAddition                             |    54,888.5 ns |   6,924.10 ns |    379.53 ns |         - |        - |          - |
| TestFloatTSubtraction                           |    55,137.4 ns |   2,792.02 ns |    153.04 ns |         - |        - |          - |
| TestUFloatTSubtraction                          |    68,427.7 ns |   3,181.75 ns |    174.40 ns |         - |        - |          - |
| TestFloatTMultiplication                        |    84,422.0 ns |   4,142.95 ns |    227.09 ns |         - |        - |          - |
| TestUFloatTMultiplication                       |    73,280.5 ns |   5,458.53 ns |    299.20 ns |         - |        - |          - |
| TestFloatTDivision                              |   478,688.7 ns |   6,438.56 ns |    352.92 ns |         - |        - |          - |
| TestUFloatTDivision                             |   947,592.1 ns |  64,110.42 ns |  3,514.11 ns |         - |        - |          - |
| TestFloatTConversion                            |    18,364.6 ns |     626.64 ns |     34.35 ns |         - |        - |          - |
| TestUFloatTConversion                           |    12,839.5 ns |   1,153.86 ns |     63.25 ns |         - |        - |          - |
