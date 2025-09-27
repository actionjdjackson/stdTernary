```

BenchmarkDotNet v0.14.0, macOS Sequoia 15.6.1 (24G90) [Darwin 24.6.0]
Apple M4 Pro, 1 CPU, 14 logical and 14 physical cores
.NET SDK 9.0.305
  [Host]     : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 8.0.20 (8.0.2025.41914), Arm64 RyuJIT AdvSIMD


```
| Method                   | Mean       | Error    | StdDev   | Allocated |
|------------------------- |-----------:|---------:|---------:|----------:|
| TestIntTAddition         |   372.4 μs |  1.33 μs |  1.24 μs |         - |
| TestIntTSubtraction      |   476.7 μs |  2.34 μs |  2.07 μs |         - |
| TestIntTMultiplication   |   525.7 μs |  2.48 μs |  2.32 μs |       1 B |
| TestIntTDivision         | 3,440.3 μs | 10.36 μs |  8.65 μs |     104 B |
| TestIntTModulus          | 3,453.1 μs | 20.87 μs | 19.52 μs |      84 B |
| TestIntTPower            |   412.6 μs |  2.08 μs |  1.95 μs |         - |
| TestFloatTAddition       |   823.6 μs |  4.98 μs |  4.66 μs |       1 B |
| TestFloatTSubtraction    |   829.6 μs |  4.96 μs |  4.64 μs |       1 B |
| TestFloatTMultiplication |   931.8 μs |  4.01 μs |  3.75 μs |       1 B |
| TestFloatTDivision       | 3,231.5 μs | 14.94 μs | 13.97 μs |     866 B |
| TestFloatTModulus        |   630.2 μs |  3.32 μs |  2.94 μs |       1 B |
