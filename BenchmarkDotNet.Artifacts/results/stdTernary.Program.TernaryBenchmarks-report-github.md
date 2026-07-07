```

BenchmarkDotNet v0.15.8, macOS Tahoe 26.4.1 (25E253) [Darwin 25.4.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                                          | DataSize | Mean           | Error        | StdDev       | Completed Work Items | Lock Contentions | Gen0      | Gen1     | Allocated  |
|------------------------------------------------ |--------- |---------------:|-------------:|-------------:|---------------------:|-----------------:|----------:|---------:|-----------:|
| **TestTernaryQuickSort**                            | **10**       | **3,778,739.7 ns** | **58,864.26 ns** | **55,061.67 ns** |                    **-** |                **-** |    **7.8125** |        **-** |    **82400 B** |
| TestBinaryQuickSort                             | 10       |   273,487.1 ns |  4,818.95 ns |  4,507.65 ns |                    - |                - |    4.8828 |        - |    42400 B |
| TestTernarySearchTree                           | 10       | 2,183,482.6 ns | 17,868.50 ns | 16,714.21 ns |                    - |                - | 1542.9688 | 113.2813 | 12926887 B |
| TestBinarySearchTree                            | 10       | 2,832,220.0 ns | 22,503.52 ns | 21,049.81 ns |                    - |                - |  250.0000 |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                   | 10       |    22,234.7 ns |    293.92 ns |    274.94 ns |                    - |                - |    2.5635 |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons    | 10       |    22,494.1 ns |    326.14 ns |    305.07 ns |                    - |                - |    2.8687 |        - |    24000 B |
| TestIfComparisons                               | 10       |    20,945.5 ns |    303.48 ns |    269.02 ns |                    - |                - |         - |        - |          - |
| TestCaseComparisons                             | 10       |    20,406.6 ns |    392.32 ns |    419.77 ns |                    - |                - |         - |        - |          - |
| TestIntTAddition                                | 10       |    62,123.1 ns |    730.76 ns |    647.80 ns |                    - |                - |         - |        - |          - |
| TestIntBAddition                                | 10       |    60,919.9 ns |    618.49 ns |    578.54 ns |                    - |                - |         - |        - |          - |
| TestUIntTAddition                               | 10       |    13,436.4 ns |    133.25 ns |    124.64 ns |                    - |                - |         - |        - |          - |
| TestIntTSubtraction                             | 10       |    77,269.5 ns |    469.08 ns |    391.70 ns |                    - |                - |         - |        - |          - |
| TestIntBSubtraction                             | 10       |    62,344.5 ns |    647.22 ns |    573.74 ns |                    - |                - |         - |        - |          - |
| TestUIntTSubtraction                            | 10       |    18,334.5 ns |    208.63 ns |    184.95 ns |                    - |                - |         - |        - |          - |
| TestIntTMultiplication                          | 10       |    89,463.3 ns |    895.91 ns |    794.20 ns |                    - |                - |         - |        - |          - |
| TestIntBMultiplication                          | 10       |   161,462.9 ns |  2,051.11 ns |  1,918.61 ns |                    - |                - |         - |        - |          - |
| TestUIntTMultiplication                         | 10       |    38,861.2 ns |    590.90 ns |    552.72 ns |                    - |                - |         - |        - |          - |
| TestIntTDivision                                | 10       |   499,139.8 ns |  4,985.75 ns |  4,419.73 ns |                    - |                - |         - |        - |       16 B |
| TestIntBDivision                                | 10       |   397,941.8 ns |  4,269.38 ns |  3,784.69 ns |                    - |                - |         - |        - |        9 B |
| TestUIntTDivision                               | 10       |   236,009.6 ns |  3,294.73 ns |  3,081.90 ns |                    - |                - |         - |        - |          - |
| TestIntTModulus                                 | 10       |   501,764.8 ns |  5,762.53 ns |  5,108.33 ns |                    - |                - |         - |        - |       14 B |
| TestIntBModulus                                 | 10       |   399,297.3 ns |  5,652.00 ns |  5,286.88 ns |                    - |                - |         - |        - |        9 B |
| TestUIntTModulus                                | 10       |   234,574.0 ns |  1,930.96 ns |  1,806.22 ns |                    - |                - |         - |        - |          - |
| TestIntTPower                                   | 10       |    61,742.3 ns |    924.34 ns |    864.63 ns |                    - |                - |         - |        - |          - |
| TestUIntTBitwise                                | 10       |    30,580.3 ns |    349.57 ns |    326.99 ns |                    - |                - |         - |        - |          - |
| TestTryteArithmetic                             | 10       |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestUTryteArithmetic                            | 10       |    45,109.9 ns |    612.29 ns |    572.74 ns |                    - |                - |         - |        - |          - |
| TestTryteBitwise                                | 10       |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestUTryteBitwise                               | 10       |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestTryteVsByteConversion                       | 10       |     6,367.8 ns |     94.94 ns |     88.80 ns |                    - |                - |         - |        - |          - |
| TestUTryteVsByteConversion                      | 10       |     1,553.9 ns |     11.20 ns |      9.35 ns |                    - |                - |         - |        - |          - |
| TestTritOperations                              | 10       |       454.9 ns |      4.03 ns |      3.77 ns |                    - |                - |         - |        - |          - |
| TestUTritOperations                             | 10       |       962.6 ns |      7.08 ns |      6.62 ns |                    - |                - |         - |        - |          - |
| TestTritDecisionMaking                          | 10       |       797.6 ns |      6.44 ns |      6.02 ns |                    - |                - |         - |        - |          - |
| TestMixedBalancedUnbalancedSpaceshipComparisons | 10       |    22,305.0 ns |    341.36 ns |    319.31 ns |                    - |                - |         - |        - |          - |
| TestFloatTAddition                              | 10       |   147,544.4 ns |  1,011.99 ns |    845.06 ns |                    - |                - |         - |        - |          - |
| TestUFloatTAddition                             | 10       |    55,384.3 ns |  1,069.24 ns |  1,313.12 ns |                    - |                - |         - |        - |          - |
| TestFloatTSubtraction                           | 10       |   149,521.1 ns |  2,758.70 ns |  2,709.41 ns |                    - |                - |         - |        - |          - |
| TestUFloatTSubtraction                          | 10       |    68,006.2 ns |    697.10 ns |    652.07 ns |                    - |                - |         - |        - |          - |
| TestFloatTMultiplication                        | 10       |   176,164.9 ns |  1,211.42 ns |  1,073.89 ns |                    - |                - |         - |        - |          - |
| TestUFloatTMultiplication                       | 10       |    72,178.5 ns |    564.17 ns |    527.73 ns |                    - |                - |         - |        - |          - |
| TestFloatTDivision                              | 10       |   586,296.0 ns | 10,565.24 ns | 10,376.48 ns |                    - |                - |         - |        - |          - |
| TestUFloatTDivision                             | 10       |   947,583.9 ns | 15,548.52 ns | 12,983.72 ns |                    - |                - |         - |        - |          - |
| TestFloatTConversion                            | 10       |    57,824.1 ns |  1,061.99 ns |    993.39 ns |                    - |                - |         - |        - |          - |
| TestUFloatTConversion                           | 10       |    12,744.4 ns |     98.16 ns |     91.82 ns |                    - |                - |         - |        - |          - |
| TestTernaryQuickSortLarge                       | 10       |     2,803.6 ns |     50.48 ns |     47.22 ns |                    - |                - |    0.0114 |        - |      104 B |
| TestBinaryQuickSortLarge                        | 10       |       126.6 ns |      1.78 ns |      1.67 ns |                    - |                - |    0.0076 |        - |       64 B |
| TestTernarySearchTreeMemory                     | 10       |     1,832.5 ns |     11.83 ns |     11.06 ns |                    - |                - |    1.2169 |   0.0038 |    10192 B |
| TestBinarySearchTreeMemory                      | 10       |       576.6 ns |      4.07 ns |      3.61 ns |                    - |                - |    0.2556 |   0.0010 |     2144 B |
| **TestTernaryQuickSort**                            | **100**      | **3,901,521.4 ns** | **73,137.75 ns** | **71,831.05 ns** |                    **-** |                **-** |    **7.8125** |        **-** |    **82400 B** |
| TestBinaryQuickSort                             | 100      |   272,506.0 ns |  2,101.45 ns |  1,862.88 ns |                    - |                - |    4.8828 |        - |    42400 B |
| TestTernarySearchTree                           | 100      | 2,179,460.5 ns | 21,373.26 ns | 19,992.56 ns |                    - |                - | 1542.9688 | 109.3750 | 12932688 B |
| TestBinarySearchTree                            | 100      | 2,793,347.2 ns | 21,069.16 ns | 19,708.11 ns |                    - |                - |  250.0000 |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                   | 100      |    22,033.9 ns |    396.32 ns |    370.72 ns |                    - |                - |    2.5635 |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons    | 100      |    22,420.5 ns |    345.97 ns |    323.62 ns |                    - |                - |    2.8687 |        - |    24000 B |
| TestIfComparisons                               | 100      |    21,321.0 ns |    406.70 ns |    360.53 ns |                    - |                - |         - |        - |          - |
| TestCaseComparisons                             | 100      |    20,745.1 ns |    400.38 ns |    428.40 ns |                    - |                - |         - |        - |          - |
| TestIntTAddition                                | 100      |    61,452.3 ns |    486.16 ns |    430.97 ns |                    - |                - |         - |        - |          - |
| TestIntBAddition                                | 100      |    60,140.2 ns |    721.45 ns |    639.55 ns |                    - |                - |         - |        - |          - |
| TestUIntTAddition                               | 100      |    14,185.0 ns |    137.39 ns |    121.79 ns |                    - |                - |         - |        - |          - |
| TestIntTSubtraction                             | 100      |    77,348.7 ns |    689.13 ns |    610.89 ns |                    - |                - |         - |        - |          - |
| TestIntBSubtraction                             | 100      |    62,626.7 ns |    566.69 ns |    502.35 ns |                    - |                - |         - |        - |          - |
| TestUIntTSubtraction                            | 100      |    18,263.4 ns |    169.55 ns |    150.30 ns |                    - |                - |         - |        - |          - |
| TestIntTMultiplication                          | 100      |    88,906.0 ns |  1,018.56 ns |    902.92 ns |                    - |                - |         - |        - |          - |
| TestIntBMultiplication                          | 100      |   161,196.0 ns |  1,121.96 ns |    994.59 ns |                    - |                - |         - |        - |          - |
| TestUIntTMultiplication                         | 100      |    38,574.8 ns |    533.29 ns |    498.84 ns |                    - |                - |         - |        - |          - |
| TestIntTDivision                                | 100      |   500,079.5 ns |  6,362.72 ns |  5,951.69 ns |                    - |                - |         - |        - |       18 B |
| TestIntBDivision                                | 100      |   397,478.8 ns |  3,806.86 ns |  3,374.69 ns |                    - |                - |         - |        - |       10 B |
| TestUIntTDivision                               | 100      |   233,414.0 ns |  3,113.93 ns |  2,912.77 ns |                    - |                - |         - |        - |          - |
| TestIntTModulus                                 | 100      |   500,345.0 ns |  6,261.10 ns |  5,856.64 ns |                    - |                - |         - |        - |       15 B |
| TestIntBModulus                                 | 100      |   395,559.6 ns |  3,202.46 ns |  2,838.90 ns |                    - |                - |         - |        - |       10 B |
| TestUIntTModulus                                | 100      |   242,310.6 ns |  3,825.56 ns |  3,578.43 ns |                    - |                - |         - |        - |          - |
| TestIntTPower                                   | 100      |    62,230.5 ns |  1,013.08 ns |    898.07 ns |                    - |                - |         - |        - |          - |
| TestUIntTBitwise                                | 100      |    30,382.9 ns |    408.37 ns |    381.99 ns |                    - |                - |         - |        - |          - |
| TestTryteArithmetic                             | 100      |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestUTryteArithmetic                            | 100      |    44,996.1 ns |    545.60 ns |    510.36 ns |                    - |                - |         - |        - |          - |
| TestTryteBitwise                                | 100      |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestUTryteBitwise                               | 100      |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestTryteVsByteConversion                       | 100      |     6,297.4 ns |     32.88 ns |     25.67 ns |                    - |                - |         - |        - |          - |
| TestUTryteVsByteConversion                      | 100      |     1,548.2 ns |     19.81 ns |     18.53 ns |                    - |                - |         - |        - |          - |
| TestTritOperations                              | 100      |       448.8 ns |      2.04 ns |      1.80 ns |                    - |                - |         - |        - |          - |
| TestUTritOperations                             | 100      |       959.9 ns |     10.72 ns |     10.03 ns |                    - |                - |         - |        - |          - |
| TestTritDecisionMaking                          | 100      |       795.8 ns |      7.67 ns |      7.18 ns |                    - |                - |         - |        - |          - |
| TestMixedBalancedUnbalancedSpaceshipComparisons | 100      |    21,913.7 ns |    267.30 ns |    236.96 ns |                    - |                - |         - |        - |          - |
| TestFloatTAddition                              | 100      |   145,972.5 ns |    929.20 ns |    823.71 ns |                    - |                - |         - |        - |          - |
| TestUFloatTAddition                             | 100      |    53,480.7 ns |    374.61 ns |    332.08 ns |                    - |                - |         - |        - |          - |
| TestFloatTSubtraction                           | 100      |   148,881.4 ns |  1,296.64 ns |  1,149.43 ns |                    - |                - |         - |        - |          - |
| TestUFloatTSubtraction                          | 100      |    68,444.8 ns |    670.19 ns |    626.90 ns |                    - |                - |         - |        - |          - |
| TestFloatTMultiplication                        | 100      |   173,512.3 ns |  2,171.11 ns |  1,924.63 ns |                    - |                - |         - |        - |          - |
| TestUFloatTMultiplication                       | 100      |    72,235.8 ns |    592.86 ns |    554.56 ns |                    - |                - |         - |        - |          - |
| TestFloatTDivision                              | 100      |   570,095.7 ns |  4,676.65 ns |  3,905.22 ns |                    - |                - |         - |        - |          - |
| TestUFloatTDivision                             | 100      |   924,456.1 ns |  7,325.86 ns |  6,852.62 ns |                    - |                - |         - |        - |          - |
| TestFloatTConversion                            | 100      |    56,094.6 ns |    235.79 ns |    184.09 ns |                    - |                - |         - |        - |          - |
| TestUFloatTConversion                           | 100      |    12,638.1 ns |    131.62 ns |    116.68 ns |                    - |                - |         - |        - |          - |
| TestTernaryQuickSortLarge                       | 100      |    38,249.8 ns |    551.42 ns |    541.56 ns |                    - |                - |    0.0610 |        - |      824 B |
| TestBinaryQuickSortLarge                        | 100      |     2,691.8 ns |     27.01 ns |     23.94 ns |                    - |                - |    0.0496 |        - |      424 B |
| TestTernarySearchTreeMemory                     | 100      |    22,241.9 ns |    171.79 ns |    160.70 ns |                    - |                - |   13.8550 |   0.2747 |   116032 B |
| TestBinarySearchTreeMemory                      | 100      |     9,143.2 ns |     82.92 ns |     77.57 ns |                    - |                - |    2.5787 |   0.0763 |    21616 B |
| **TestTernaryQuickSort**                            | **1000**     | **3,816,190.3 ns** | **49,998.00 ns** | **44,321.92 ns** |                    **-** |                **-** |    **7.8125** |        **-** |    **82400 B** |
| TestBinaryQuickSort                             | 1000     |   273,749.2 ns |  3,695.51 ns |  3,456.79 ns |                    - |                - |    4.8828 |        - |    42400 B |
| TestTernarySearchTree                           | 1000     | 2,147,731.8 ns | 19,430.85 ns | 17,224.94 ns |                    - |                - | 1542.9688 | 109.3750 | 12925135 B |
| TestBinarySearchTree                            | 1000     | 2,783,896.0 ns | 21,630.16 ns | 20,232.86 ns |                    - |                - |  250.0000 |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                   | 1000     |    22,470.6 ns |    189.31 ns |    167.82 ns |                    - |                - |    2.5635 |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons    | 1000     |    22,792.1 ns |    439.64 ns |    411.24 ns |                    - |                - |    2.8687 |        - |    24000 B |
| TestIfComparisons                               | 1000     |    20,836.7 ns |    407.47 ns |    418.44 ns |                    - |                - |         - |        - |          - |
| TestCaseComparisons                             | 1000     |    20,251.1 ns |    367.60 ns |    343.85 ns |                    - |                - |         - |        - |          - |
| TestIntTAddition                                | 1000     |    61,646.1 ns |    619.95 ns |    579.90 ns |                    - |                - |         - |        - |          - |
| TestIntBAddition                                | 1000     |    59,843.5 ns |    671.32 ns |    627.95 ns |                    - |                - |         - |        - |          - |
| TestUIntTAddition                               | 1000     |    13,331.1 ns |    151.79 ns |    141.98 ns |                    - |                - |         - |        - |          - |
| TestIntTSubtraction                             | 1000     |    76,845.1 ns |    967.70 ns |    857.84 ns |                    - |                - |         - |        - |          - |
| TestIntBSubtraction                             | 1000     |    62,404.1 ns |    893.65 ns |    835.92 ns |                    - |                - |         - |        - |          - |
| TestUIntTSubtraction                            | 1000     |    18,957.5 ns |    157.59 ns |    139.70 ns |                    - |                - |         - |        - |          - |
| TestIntTMultiplication                          | 1000     |    88,726.5 ns |  1,264.29 ns |  1,182.62 ns |                    - |                - |         - |        - |          - |
| TestIntBMultiplication                          | 1000     |   160,604.4 ns |  1,602.78 ns |  1,499.24 ns |                    - |                - |         - |        - |          - |
| TestUIntTMultiplication                         | 1000     |    38,064.0 ns |    318.99 ns |    266.37 ns |                    - |                - |         - |        - |          - |
| TestIntTDivision                                | 1000     |   499,431.0 ns |  7,542.12 ns |  6,685.89 ns |                    - |                - |         - |        - |       16 B |
| TestIntBDivision                                | 1000     |   396,949.3 ns |  4,106.71 ns |  3,841.42 ns |                    - |                - |         - |        - |       12 B |
| TestUIntTDivision                               | 1000     |   231,480.9 ns |  2,646.49 ns |  2,475.53 ns |                    - |                - |         - |        - |          - |
| TestIntTModulus                                 | 1000     |   494,936.5 ns |  3,976.29 ns |  3,320.38 ns |                    - |                - |         - |        - |       18 B |
| TestIntBModulus                                 | 1000     |   394,156.3 ns |  3,899.12 ns |  3,647.24 ns |                    - |                - |         - |        - |       10 B |
| TestUIntTModulus                                | 1000     |   231,214.1 ns |  2,386.91 ns |  2,232.71 ns |                    - |                - |         - |        - |          - |
| TestIntTPower                                   | 1000     |    60,812.7 ns |    745.77 ns |    697.60 ns |                    - |                - |         - |        - |          - |
| TestUIntTBitwise                                | 1000     |    30,543.0 ns |    394.90 ns |    369.39 ns |                    - |                - |         - |        - |          - |
| TestTryteArithmetic                             | 1000     |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestUTryteArithmetic                            | 1000     |    44,746.3 ns |    492.86 ns |    461.02 ns |                    - |                - |         - |        - |          - |
| TestTryteBitwise                                | 1000     |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestUTryteBitwise                               | 1000     |             NA |           NA |           NA |                   NA |               NA |        NA |       NA |         NA |
| TestTryteVsByteConversion                       | 1000     |     6,224.0 ns |     44.64 ns |     39.57 ns |                    - |                - |         - |        - |          - |
| TestUTryteVsByteConversion                      | 1000     |     1,534.8 ns |     17.12 ns |     16.01 ns |                    - |                - |         - |        - |          - |
| TestTritOperations                              | 1000     |       448.9 ns |      4.42 ns |      4.14 ns |                    - |                - |         - |        - |          - |
| TestUTritOperations                             | 1000     |       946.9 ns |      5.74 ns |      5.37 ns |                    - |                - |         - |        - |          - |
| TestTritDecisionMaking                          | 1000     |       787.8 ns |     11.40 ns |     10.66 ns |                    - |                - |         - |        - |          - |
| TestMixedBalancedUnbalancedSpaceshipComparisons | 1000     |    21,985.4 ns |    252.00 ns |    235.72 ns |                    - |                - |         - |        - |          - |
| TestFloatTAddition                              | 1000     |   147,439.9 ns |    945.00 ns |    789.12 ns |                    - |                - |         - |        - |          - |
| TestUFloatTAddition                             | 1000     |    53,434.1 ns |    596.49 ns |    557.96 ns |                    - |                - |         - |        - |          - |
| TestFloatTSubtraction                           | 1000     |   146,548.7 ns |  1,289.53 ns |  1,076.82 ns |                    - |                - |         - |        - |          - |
| TestUFloatTSubtraction                          | 1000     |    67,134.4 ns |    760.29 ns |    711.17 ns |                    - |                - |         - |        - |          - |
| TestFloatTMultiplication                        | 1000     |   175,356.4 ns |    980.45 ns |    818.72 ns |                    - |                - |         - |        - |          - |
| TestUFloatTMultiplication                       | 1000     |    71,694.1 ns |    819.20 ns |    766.28 ns |                    - |                - |         - |        - |          - |
| TestFloatTDivision                              | 1000     |   573,240.0 ns |  3,542.71 ns |  3,140.52 ns |                    - |                - |         - |        - |          - |
| TestUFloatTDivision                             | 1000     |   926,699.4 ns |  8,296.68 ns |  7,354.79 ns |                    - |                - |         - |        - |          - |
| TestFloatTConversion                            | 1000     |    56,853.7 ns |    538.77 ns |    477.61 ns |                    - |                - |         - |        - |          - |
| TestUFloatTConversion                           | 1000     |    12,463.9 ns |    135.64 ns |    126.88 ns |                    - |                - |         - |        - |          - |
| TestTernaryQuickSortLarge                       | 1000     |   502,885.6 ns | 10,032.93 ns | 11,151.57 ns |                    - |                - |         - |        - |     8024 B |
| TestBinaryQuickSortLarge                        | 1000     |    36,695.4 ns |    408.56 ns |    341.16 ns |                    - |                - |    0.4272 |        - |     4024 B |
| TestTernarySearchTreeMemory                     | 1000     |   284,253.9 ns |  2,666.57 ns |  2,494.31 ns |                    - |                - |  168.4570 |  25.3906 |  1412032 B |
| TestBinarySearchTreeMemory                      | 1000     |   132,216.8 ns |  1,636.15 ns |  1,530.46 ns |                    - |                - |   25.6348 |   6.1035 |   215984 B |

Benchmarks with issues:
  TernaryBenchmarks.TestTryteArithmetic: DefaultJob [DataSize=10]
  TernaryBenchmarks.TestTryteBitwise: DefaultJob [DataSize=10]
  TernaryBenchmarks.TestUTryteBitwise: DefaultJob [DataSize=10]
  TernaryBenchmarks.TestTryteArithmetic: DefaultJob [DataSize=100]
  TernaryBenchmarks.TestTryteBitwise: DefaultJob [DataSize=100]
  TernaryBenchmarks.TestUTryteBitwise: DefaultJob [DataSize=100]
  TernaryBenchmarks.TestTryteArithmetic: DefaultJob [DataSize=1000]
  TernaryBenchmarks.TestTryteBitwise: DefaultJob [DataSize=1000]
  TernaryBenchmarks.TestUTryteBitwise: DefaultJob [DataSize=1000]
