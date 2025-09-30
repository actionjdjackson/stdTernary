```

BenchmarkDotNet v0.14.0, Debian GNU/Linux 12 (bookworm) (container)
Intel Xeon Platinum 8370C CPU 2.80GHz, 1 CPU, 2 logical cores and 1 physical core
.NET SDK 8.0.413
  [Host]     : .NET 8.0.19 (8.0.1925.36514), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  DefaultJob : .NET 8.0.19 (8.0.1925.36514), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI


```
| Method                | Mean       | Error     | StdDev    | Gen0      | Gen1     | Allocated   |
|---------------------- |-----------:|----------:|----------:|----------:|---------:|------------:|
| TestTernaryQuickSort  | 7,583.4 μs | 141.61 μs | 139.08 μs |         - |        - |    80.47 KB |
| TestBinaryQuickSort   |   523.1 μs |  10.41 μs |  15.58 μs |    0.9766 |        - |    41.41 KB |
| TestTernarySearchTree | 9,615.2 μs | 190.96 μs | 525.95 μs | 1468.7500 | 109.3750 | 36147.44 KB |
| TestBinarySearchTree  | 8,248.0 μs | 162.77 μs | 370.71 μs |   78.1250 |        - |  2057.04 KB |
