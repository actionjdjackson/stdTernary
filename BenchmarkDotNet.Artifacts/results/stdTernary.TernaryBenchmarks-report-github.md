```

BenchmarkDotNet v0.15.8, macOS Sequoia 15.6.1 (24G90) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 10.0.101
  [Host]     : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.1 (10.0.1, 10.0.125.57005), Arm64 RyuJIT armv8.0-a


```
| Method                                       | DataSize | Mean           | Error        | StdDev       | Gen0      | Completed Work Items | Lock Contentions | Gen1     | Allocated  |
|--------------------------------------------- |--------- |---------------:|-------------:|-------------:|----------:|---------------------:|-----------------:|---------:|-----------:|
| **TestTernaryQuickSort**                         | **10**       | **4,591,790.5 ns** | **68,774.97 ns** | **64,332.15 ns** |    **7.8125** |                    **-** |                **-** |        **-** |    **82400 B** |
| TestBinaryQuickSort                          | 10       |   311,819.7 ns |  2,537.61 ns |  2,373.68 ns |    4.8828 |                    - |                - |        - |    42400 B |
| TestTernarySearchTree                        | 10       | 2,477,739.0 ns | 24,550.83 ns | 22,964.86 ns | 1542.9688 |                    - |                - | 109.3750 | 12921440 B |
| TestBinarySearchTree                         | 10       | 3,105,251.0 ns | 36,333.99 ns | 33,986.83 ns |  250.0000 |                    - |                - |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                | 10       |    24,254.3 ns |    226.09 ns |    200.42 ns |    2.5635 |                    - |                - |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons | 10       |    25,111.0 ns |    287.17 ns |    268.62 ns |    2.8687 |                    - |                - |        - |    24000 B |
| TestIfComparisons                            | 10       |    22,680.0 ns |    424.27 ns |    396.87 ns |         - |                    - |                - |        - |          - |
| TestCaseComparisons                          | 10       |    22,058.3 ns |    161.59 ns |    151.15 ns |         - |                    - |                - |        - |          - |
| TestIntTAddition                             | 10       |    69,970.1 ns |    939.36 ns |    878.68 ns |         - |                    - |                - |        - |          - |
| TestIntBAddition                             | 10       |    67,671.9 ns |    541.43 ns |    506.45 ns |         - |                    - |                - |        - |          - |
| TestIntTSubtraction                          | 10       |    88,095.5 ns |    737.41 ns |    689.77 ns |         - |                    - |                - |        - |          - |
| TestIntBSubtraction                          | 10       |    70,009.7 ns |    597.48 ns |    558.89 ns |         - |                    - |                - |        - |          - |
| TestIntTMultiplication                       | 10       |   101,755.2 ns |  1,125.00 ns |  1,052.33 ns |         - |                    - |                - |        - |          - |
| TestIntBMultiplication                       | 10       |   177,719.2 ns |  1,647.84 ns |  1,541.39 ns |         - |                    - |                - |        - |          - |
| TestIntTDivision                             | 10       |   567,975.0 ns |  6,606.90 ns |  5,856.85 ns |         - |                    - |                - |        - |       17 B |
| TestIntBDivision                             | 10       |   452,052.5 ns |  2,901.85 ns |  2,714.39 ns |         - |                    - |                - |        - |       10 B |
| TestIntTModulus                              | 10       |   576,638.5 ns |  3,606.04 ns |  3,196.66 ns |         - |                    - |                - |        - |       15 B |
| TestIntBModulus                              | 10       |   441,709.2 ns |  4,053.63 ns |  3,791.77 ns |         - |                    - |                - |        - |       11 B |
| TestIntTPower                                | 10       |    69,302.6 ns |    601.13 ns |    562.30 ns |         - |                    - |                - |        - |          - |
| TestTryteArithmetic                          | 10       |             NA |           NA |           NA |        NA |                   NA |               NA |       NA |         NA |
| TestTryteBitwise                             | 10       |             NA |           NA |           NA |        NA |                   NA |               NA |       NA |         NA |
| TestTryteVsByteConversion                    | 10       |     7,184.8 ns |     67.97 ns |     63.58 ns |         - |                    - |                - |        - |          - |
| TestTritOperations                           | 10       |       494.1 ns |      5.60 ns |      5.24 ns |         - |                    - |                - |        - |          - |
| TestTritDecisionMaking                       | 10       |       852.0 ns |      3.78 ns |      3.53 ns |         - |                    - |                - |        - |          - |
| TestFloatTAddition                           | 10       |   166,182.6 ns |  2,075.91 ns |  1,941.81 ns |         - |                    - |                - |        - |          - |
| TestFloatTSubtraction                        | 10       |   166,483.6 ns |  1,592.87 ns |  1,489.97 ns |         - |                    - |                - |        - |          - |
| TestFloatTMultiplication                     | 10       |   197,674.9 ns |  1,909.29 ns |  1,785.95 ns |         - |                    - |                - |        - |          - |
| TestFloatTDivision                           | 10       |   653,524.2 ns |  6,993.84 ns |  6,542.05 ns |         - |                    - |                - |        - |          - |
| TestFloatTConversion                         | 10       |    65,630.0 ns |    809.93 ns |    757.61 ns |         - |                    - |                - |        - |          - |
| TestTernaryQuickSortLarge                    | 10       |     3,176.8 ns |     15.89 ns |     13.27 ns |    0.0114 |                    - |                - |        - |      104 B |
| TestBinaryQuickSortLarge                     | 10       |       141.4 ns |      1.01 ns |      0.95 ns |    0.0076 |                    - |                - |        - |       64 B |
| TestTernarySearchTreeMemory                  | 10       |     1,993.6 ns |     16.35 ns |     15.30 ns |    1.2169 |                    - |                - |   0.0038 |    10192 B |
| TestBinarySearchTreeMemory                   | 10       |       623.9 ns |      3.18 ns |      2.97 ns |    0.2556 |                    - |                - |   0.0010 |     2144 B |
| **TestTernaryQuickSort**                         | **100**      | **4,288,692.5 ns** | **43,534.83 ns** | **40,722.50 ns** |    **7.8125** |                    **-** |                **-** |        **-** |    **82400 B** |
| TestBinaryQuickSort                          | 100      |   313,771.9 ns |  3,612.06 ns |  3,378.73 ns |    4.8828 |                    - |                - |        - |    42400 B |
| TestTernarySearchTree                        | 100      | 2,420,418.3 ns | 28,226.01 ns | 25,021.62 ns | 1542.9688 |                    - |                - | 109.3750 | 12924236 B |
| TestBinarySearchTree                         | 100      | 3,094,508.1 ns | 14,566.85 ns | 13,625.84 ns |  250.0000 |                    - |                - |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                | 100      |    24,111.4 ns |    173.13 ns |    161.94 ns |    2.5635 |                    - |                - |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons | 100      |    25,114.0 ns |    184.54 ns |    172.62 ns |    2.8687 |                    - |                - |        - |    24000 B |
| TestIfComparisons                            | 100      |    22,289.1 ns |    229.74 ns |    214.90 ns |         - |                    - |                - |        - |          - |
| TestCaseComparisons                          | 100      |    22,509.8 ns |    362.11 ns |    338.72 ns |         - |                    - |                - |        - |          - |
| TestIntTAddition                             | 100      |    69,892.4 ns |    759.73 ns |    710.65 ns |         - |                    - |                - |        - |          - |
| TestIntBAddition                             | 100      |    68,661.6 ns |    467.65 ns |    437.44 ns |         - |                    - |                - |        - |          - |
| TestIntTSubtraction                          | 100      |    86,709.5 ns |    635.09 ns |    562.99 ns |         - |                    - |                - |        - |          - |
| TestIntBSubtraction                          | 100      |    70,970.4 ns |  1,238.27 ns |  1,097.69 ns |         - |                    - |                - |        - |          - |
| TestIntTMultiplication                       | 100      |   101,930.1 ns |    621.76 ns |    581.59 ns |         - |                    - |                - |        - |          - |
| TestIntBMultiplication                       | 100      |   176,588.1 ns |  1,285.17 ns |  1,202.15 ns |         - |                    - |                - |        - |          - |
| TestIntTDivision                             | 100      |   563,637.0 ns |  5,917.77 ns |  4,941.61 ns |         - |                    - |                - |        - |       15 B |
| TestIntBDivision                             | 100      |   447,342.4 ns |  3,794.13 ns |  3,549.03 ns |         - |                    - |                - |        - |       11 B |
| TestIntTModulus                              | 100      |   576,187.0 ns |  4,772.25 ns |  4,230.48 ns |         - |                    - |                - |        - |       15 B |
| TestIntBModulus                              | 100      |   439,095.4 ns |  5,986.65 ns |  5,307.01 ns |         - |                    - |                - |        - |       12 B |
| TestIntTPower                                | 100      |    68,985.7 ns |    578.28 ns |    482.89 ns |         - |                    - |                - |        - |          - |
| TestTryteArithmetic                          | 100      |             NA |           NA |           NA |        NA |                   NA |               NA |       NA |         NA |
| TestTryteBitwise                             | 100      |             NA |           NA |           NA |        NA |                   NA |               NA |       NA |         NA |
| TestTryteVsByteConversion                    | 100      |     7,231.3 ns |     71.33 ns |     66.72 ns |         - |                    - |                - |        - |          - |
| TestTritOperations                           | 100      |       503.5 ns |      9.48 ns |      8.87 ns |         - |                    - |                - |        - |          - |
| TestTritDecisionMaking                       | 100      |       869.4 ns |     14.20 ns |     13.28 ns |         - |                    - |                - |        - |          - |
| TestFloatTAddition                           | 100      |   170,368.9 ns |  2,580.99 ns |  2,414.26 ns |         - |                    - |                - |        - |          - |
| TestFloatTSubtraction                        | 100      |   171,859.1 ns |  3,073.68 ns |  2,875.12 ns |         - |                    - |                - |        - |          - |
| TestFloatTMultiplication                     | 100      |   203,785.3 ns |  3,537.59 ns |  3,309.06 ns |         - |                    - |                - |        - |          - |
| TestFloatTDivision                           | 100      |   662,956.8 ns | 12,212.04 ns | 11,423.15 ns |         - |                    - |                - |        - |          - |
| TestFloatTConversion                         | 100      |    66,605.4 ns |  1,304.28 ns |  1,649.49 ns |         - |                    - |                - |        - |          - |
| TestTernaryQuickSortLarge                    | 100      |    44,050.7 ns |    827.39 ns |    773.94 ns |    0.0610 |                    - |                - |        - |      824 B |
| TestBinaryQuickSortLarge                     | 100      |     3,196.2 ns |     48.07 ns |     44.96 ns |    0.0496 |                    - |                - |        - |      424 B |
| TestTernarySearchTreeMemory                  | 100      |    27,260.1 ns |    400.83 ns |    374.94 ns |   16.1438 |                    - |                - |   0.3357 |   135232 B |
| TestBinarySearchTreeMemory                   | 100      |     9,857.6 ns |    135.80 ns |    127.03 ns |    2.5787 |                    - |                - |   0.0763 |    21584 B |
| **TestTernaryQuickSort**                         | **1000**     | **4,421,655.6 ns** | **65,797.51 ns** | **61,547.03 ns** |    **7.8125** |                    **-** |                **-** |        **-** |    **82400 B** |
| TestBinaryQuickSort                          | 1000     |   314,791.4 ns |  2,468.43 ns |  2,308.97 ns |    4.8828 |                    - |                - |        - |    42400 B |
| TestTernarySearchTree                        | 1000     | 2,416,566.9 ns | 15,462.44 ns | 14,463.58 ns | 1542.9688 |                    - |                - | 109.3750 | 12922783 B |
| TestBinarySearchTree                         | 1000     | 3,103,064.9 ns | 17,530.80 ns | 16,398.32 ns |  250.0000 |                    - |                - |   3.9063 |  2106400 B |
| TestMethodChainingComparisons                | 1000     |    24,889.2 ns |    225.44 ns |    188.25 ns |    2.5635 |                    - |                - |        - |    21600 B |
| TestMethodChainingTernaryDecisionComparisons | 1000     |    24,270.0 ns |    155.92 ns |    138.22 ns |    2.8687 |                    - |                - |        - |    24000 B |
| TestIfComparisons                            | 1000     |    22,207.9 ns |    208.90 ns |    195.41 ns |         - |                    - |                - |        - |          - |
| TestCaseComparisons                          | 1000     |    22,325.0 ns |    172.44 ns |    152.87 ns |         - |                    - |                - |        - |          - |
| TestIntTAddition                             | 1000     |    70,703.6 ns |    860.36 ns |    762.68 ns |         - |                    - |                - |        - |          - |
| TestIntBAddition                             | 1000     |    67,656.4 ns |    478.49 ns |    447.58 ns |         - |                    - |                - |        - |          - |
| TestIntTSubtraction                          | 1000     |    86,993.3 ns |  1,048.32 ns |    980.59 ns |         - |                    - |                - |        - |          - |
| TestIntBSubtraction                          | 1000     |    71,314.8 ns |    658.90 ns |    616.34 ns |         - |                    - |                - |        - |          - |
| TestIntTMultiplication                       | 1000     |   101,688.9 ns |    965.29 ns |    902.93 ns |         - |                    - |                - |        - |          - |
| TestIntBMultiplication                       | 1000     |   176,240.2 ns |  1,722.32 ns |  1,611.06 ns |         - |                    - |                - |        - |          - |
| TestIntTDivision                             | 1000     |   571,905.0 ns |  3,673.84 ns |  3,436.51 ns |         - |                    - |                - |        - |       16 B |
| TestIntBDivision                             | 1000     |   445,025.3 ns |  3,122.33 ns |  2,920.63 ns |         - |                    - |                - |        - |       12 B |
| TestIntTModulus                              | 1000     |   570,435.0 ns |  2,483.33 ns |  2,322.91 ns |         - |                    - |                - |        - |       14 B |
| TestIntBModulus                              | 1000     |   445,457.7 ns |  3,441.11 ns |  3,218.82 ns |         - |                    - |                - |        - |       10 B |
| TestIntTPower                                | 1000     |    70,275.6 ns |    811.97 ns |    759.52 ns |         - |                    - |                - |        - |          - |
| TestTryteArithmetic                          | 1000     |             NA |           NA |           NA |        NA |                   NA |               NA |       NA |         NA |
| TestTryteBitwise                             | 1000     |             NA |           NA |           NA |        NA |                   NA |               NA |       NA |         NA |
| TestTryteVsByteConversion                    | 1000     |     7,259.0 ns |     48.02 ns |     44.91 ns |         - |                    - |                - |        - |          - |
| TestTritOperations                           | 1000     |       494.0 ns |      3.72 ns |      3.48 ns |         - |                    - |                - |        - |          - |
| TestTritDecisionMaking                       | 1000     |       876.2 ns |      4.65 ns |      3.89 ns |         - |                    - |                - |        - |          - |
| TestFloatTAddition                           | 1000     |   165,532.7 ns |  1,860.73 ns |  1,740.53 ns |         - |                    - |                - |        - |          - |
| TestFloatTSubtraction                        | 1000     |   171,080.4 ns |  1,326.61 ns |  1,240.91 ns |         - |                    - |                - |        - |          - |
| TestFloatTMultiplication                     | 1000     |   198,796.5 ns |  1,157.18 ns |  1,082.43 ns |         - |                    - |                - |        - |          - |
| TestFloatTDivision                           | 1000     |   655,576.7 ns |  5,921.33 ns |  5,538.81 ns |         - |                    - |                - |        - |          - |
| TestFloatTConversion                         | 1000     |    64,610.0 ns |    387.03 ns |    362.03 ns |         - |                    - |                - |        - |          - |
| TestTernaryQuickSortLarge                    | 1000     |   576,693.7 ns |  9,402.18 ns |  8,794.80 ns |         - |                    - |                - |        - |     8024 B |
| TestBinaryQuickSortLarge                     | 1000     |    42,032.0 ns |    293.05 ns |    274.12 ns |    0.4272 |                    - |                - |        - |     4024 B |
| TestTernarySearchTreeMemory                  | 1000     |   314,488.8 ns |  1,746.40 ns |  1,633.59 ns |  168.4570 |                    - |                - |  25.3906 |  1412032 B |
| TestBinarySearchTreeMemory                   | 1000     |   147,161.6 ns |    800.98 ns |    749.24 ns |   25.6348 |                    - |                - |   6.1035 |   216016 B |

Benchmarks with issues:
  TernaryBenchmarks.TestTryteArithmetic: DefaultJob [DataSize=10]
  TernaryBenchmarks.TestTryteBitwise: DefaultJob [DataSize=10]
  TernaryBenchmarks.TestTryteArithmetic: DefaultJob [DataSize=100]
  TernaryBenchmarks.TestTryteBitwise: DefaultJob [DataSize=100]
  TernaryBenchmarks.TestTryteArithmetic: DefaultJob [DataSize=1000]
  TernaryBenchmarks.TestTryteBitwise: DefaultJob [DataSize=1000]
