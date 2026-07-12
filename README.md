# stdTernary #
## A standard library for Ternary operations in C# ##
![build-and-test](https://github.com/actionjdjackson/stdTernary/actions/workflows/build-and-test.yml/badge.svg?event=push)

stdTernary brings an elegant, ternary-first, high-level programming experience to C#. It provides balanced and unbalanced ternary numeric types, logic, text, and containers with the operators and conversions you would expect from a native numeric stack, while abstracting the low-level representation so the *same* C# source can, in the future, be recompiled for balanced-ternary hardware.

Under the hood the core numeric types are no longer digit-serial. `IntT` and the other packed types are backed by a word-parallel (SWAR) balanced-ternary encoding: a full three-way `<,=,>` comparison is a single XOR-and-compare, addition and subtraction run over all 32 trits at once via the dual-rail scheme of Frieder & Luk (1973), and negation and shifts are single machine operations. The library also ships a Frieder-style **figure-of-merit** framework (`TernaryFom`) that meters how any C# region would run on native ternary hardware and relates it to a binary machine doing the same work — giving concrete, literature-grounded evidence of where ternary wins.

The library promotes the usage of mixed radix programming, using binary for binary concepts and ternary for ternary concepts. This means that boolean is great to model concepts with only 2 logical choices (True, False) and kleenean is great to model concepts with 3 choices (True, False, Unknown). Kleene logic is clear in what happens with the unknown state when using implications.

### Balanced and Unbalanced Ternary

stdTernary supports both balanced ternary and unbalanced ternary:

- **Balanced ternary** uses `-1`, `0`, and `1`, represented as `-`, `0`, and `+` in strings. The balanced types are `Trit`, `Tryte`, `IntT`, `FloatT`, and `TritPacker`.
- **Unbalanced ternary** uses `0`, `1`, and `2`, represented as `0`, `1`, and `2` in strings. The unbalanced types are prefixed with `U`: `UTrit`, `UTryte`, `UIntT`, `UFloatT`, and `UTritPacker`.

The unbalanced types implement ternary-native arithmetic, packing, tritwise operations, parsing, formatting, and conversions to/from the matching balanced types where the value is representable. Since unbalanced ternary is unsigned, operations that would produce a negative unbalanced value throw `OverflowException`.

### Type Conversion Library

The library includes a comprehensive **TernaryConverter** class that provides seamless conversion between balanced ternary types and common .NET binary types:

- **IntT** ↔ int, uint, short, ushort, byte, sbyte, long, ulong, decimal
- **FloatT** ↔ float, double, decimal  
- **Tryte** ↔ byte, sbyte, short, ushort, uint, ulong, decimal
- **Trit** ↔ bool

All conversions include proper overflow checking and error handling. See [CONVERTER_README.md](CONVERTER_README.md) for detailed documentation and examples.

### Word-parallel (SWAR) core and O(1) comparison

The packed balanced-ternary encoding stores each trit as a 2-bit two's-complement digit (`00` = 0, `01` = +1, `11` = −1; the `10` state is unused). This layout is what makes the core fast and faithful to ternary hardware:

- **`default` is valid.** `default(ulong) == 0` still decodes to the value zero, so default structs are safe.
- **Comparison is one instruction.** Flipping each field's sign bit makes the packed word *order-isomorphic* to the numeric value, so a full three-way `<,=,>` comparison is one XOR plus one unsigned compare — the software realization of the balanced-ternary property that a comparison resolves in a single instruction.
- **Addition is word-parallel.** The low bit of every field is an "occupied" rail and the high bit a "negative" rail, giving the dual-rail (P, N) form of Frieder & Luk. All trits are added at once with Boolean rail operations; carries ripple by whole words (typically 2–4 rounds), and because positive and negative carries can legitimately cancel in balanced ternary (`+-` + `+-` = `++`), overflow is decided only after the carries settle.
- **Negation is a rail swap; shifts are single machine shifts.** Multiply and divide use fast 6-trit-limb (balanced base-729) table conversions and the host multiplier, raising `OverflowException` exactly when a result exceeds 32 trits.

All other packed types (`Tryte`, `CharT`, `FloatT`, …) access the representation only through this encoding, so they inherit the O(1) comparison for free. (`TritPacker` keeps its own separate, documented layout in which `10` encodes −1.)

### Frieder-style evaluation: how would your code run on native ternary hardware?

Frieder & Luk (1973) argued that a non-binary architecture emulated on a binary host should not be judged by wall-clock speed against the host — "there is no point whatsoever to do any speed comparisons" — but by **monitoring operation counts per category, memory utilization, and relating them to a binary machine doing the same work**. `TernaryFom` implements exactly that: every `IntT` operation is metered, and one counter unit corresponds to one instruction on a hypothetical balanced-ternary register machine (a full `<,=,>` comparison is a *single* instruction there).

Wrap any C# region to get a native-hardware summary:

```csharp
var snapshot = TernaryFom.Measure("my kernel", () =>
{
    // any code that uses IntT: sorts, filters, arithmetic, DSP kernels, ...
    OptimizedQuicksort.TernaryQuicksort3Way(keys);
});

// Optional: run a binary reference doing the same work with BinaryFom enabled,
// then relate the two, as in Frieder's methodology:
Console.WriteLine(TernaryFom.Report(snapshot, binarySnapshot, keys.Length));
```

The report prints the ternary instruction mix (comparisons, add/sub, mul, div, negate, shift, tritwise), switching activity (trit flips), memory traffic in trits, and the ternary:binary relations for comparisons, control operations and digit flips. Across four sort distributions (n = 100,000), the 3-way quicksort executes roughly **30–40% fewer comparison instructions** than the same algorithm on a binary ISA (which must pay one-to-two compares to recover each three-way decision), with duplicate-heavy inputs benefiting most. Run the `stdTernary.Eval` project for the full correctness-fuzz + micro-benchmark + FoM suite.

### Optimized 3-way quicksort

`OptimizedQuicksort.TernaryQuicksort3Way` is a production-style quicksort built around the balanced-ternary comparison: a Dutch-national-flag 3-way partition driven by **one spaceship comparison per element** (the single trit routes each element to the less/equal/greater region), randomized median-of-three pivots, an insertion-sort cutoff, and recursion on the smaller side only. Duplicate keys collapse into the pivot block, giving O(n · log(distinct)) behaviour. Binary controls (`BinaryQuicksort3Way`, `BinaryQuicksort2Way`) with the same structure are provided so the FoM relations are apples-to-apples.

### Installation via Github Codespaces
- Click on the green Code button and then on the tab Codespaces. Choose "create new codespace on main".
- To build the .dll library (and the `stdTernary.Eval` project, both part of the solution) type "task build"
- To build and run the unit tests type "task test"
- To run the BenchmarkDotNet suite (`TernaryBenchmarks` + `ScalingBenchmarks`) interactively type "task benchmark"; use "task benchmark: short" for a quick pass over everything, or "task benchmark: full" for the complete run
- To run the SWAR correctness-fuzz, micro-benchmarks, and Frieder figure-of-merit study type "task eval" (or `cd stdTernary.Eval && dotnet run -c Release`)

### Limitations
- The library uses binary coded ternary (BCT) using 2 bits to represent 1 trit. This is obviously inefficient as 1 of the 4 states is ignored. Balanced ternary reserves one 2-bit state because it only needs `-1`, `0`, and `1`; unbalanced ternary reserves one 2-bit state because it only needs `0`, `1`, and `2`. We define 6 trits as 1 tryte such that all binary states of a single byte can fit in a single tryte which simplifies conversions and adds binary compatibility. A tryte (6 trits implemented as 12 bits) is packed as a binary integer. Note that balanced ternary is always signed so the usage of unsigned binary storage is just a choice of implementation.

Since the current compilation targets are all binary hardware platforms, this inefficiency is unavoidable. However, we expect that this is temporary as more and more balanced ternary instructions set architectures are being developed such as REBEL-6 and ART-9 which feature ternary assembly with ternary logic, memory and branching. With the new .net 10 featuring RISCV support, a possible compilation flow from c# to REBEL-6 (32-trit balanced ternary, binary compatible ISA) is technically possible through R2R. This is work in progress. The R2R framework allows low level binary vs ternary comparison to compute fairly the amount of memory and bitflips that are needed for both radixes to run the same code. In the meantime, the `TernaryFom`/`BinaryFom` instrumentation already realizes this binary-vs-ternary comparison in software: it counts the instructions, memory traffic, and digit flips each radix needs to run the same code, following Frieder & Luk's evaluation methodology.

### Dev Notes

**Sept 2025 (Jackson):** I completely overhauled stdTernary — no longer using an enum for storing `+`, `-`, and `0` values. Instead, I trit-pack 2-bit trits into binary unsigned integers (`uint`s or `ulong`s), while all the math is still done in ternary. All bitwise and bytewise operators have been overridden for trits and trytes; I am currently using the `*` operator on trits for XNOR/MULTIPLY. The structs are `Trit` and `Tryte` — and the `Tryte` can be modified easily to be any number of trits you want, up to 16 with the current implementation, holding a bitpacked array of trits in a `uint` (6 by default). `IntT` can have any number of total trits (up to 32, bitpacking into a 64-bit `ulong`) and works with non-multiples of trytes like 21-trit or 32-trit integers. `FloatT` can have any number of total trits — up to 32, separated into an exponent and a significand (typically 26-trit significand, 6-trit exponent). `MathT` provides most of the `Math` functions for `FloatT`s and some for `IntT`s, including `Log3`, `ILogT`, and trit increment/decrement. The `string` conversion (`+`, `-`, `0` characters) is for interoperability with the "Action Ternary Simulator" and for quick visualization of shifts and increments. The library also includes unbalanced ternary equivalents for the core balanced types using the same 2-bit-per-trit packing but with the digit set `0`, `1`, `2`: `UTryte` (configurable like `Tryte`), `UIntT` (32-trit unsigned), and `UFloatT`. `Tryte`, `FloatT`, and `IntT` expose modifiable static size values so you can "customize" them to a given number of trits.

**Jul 2026 (SWAR rewrite):** the packed representation and all core arithmetic were rewritten to be word-parallel. The 2-bit encoding is now a two's-complement digit — `00` = 0, `01` = +1, `11` = −1 (the `10` state is unused) — which makes the packed word order-isomorphic to the value. As a result a full three-way comparison is a single XOR + unsigned compare, addition/subtraction run over all trits at once with the dual-rail (P, N) form of Frieder & Luk, negation is a rail swap, and shifts are single machine shifts; multiply/divide go through fast 6-trit-limb (base-729) table conversions. This replaced the previous per-trit decode/normalize/encode loops, giving large per-operation speedups over the digit-serial version while keeping identical semantics (all existing tests pass). Alongside it, `TernaryFom`/`BinaryFom` add the Frieder figure-of-merit evaluation, `OptimizedQuicksort` adds a 3-way ternary quicksort with matched binary controls, and the `stdTernary.Eval` project provides correctness fuzzing, micro-benchmarks, and the FoM study. Note that `TritPacker` retains its original layout (`10` = −1) and is untouched.

## Detailed API Reference

### Core Types

| Type | Description | Key Properties/Methods |
|------|-------------|-------------------------|
| `Trit` | Fundamental ternary unit representing -1, 0, or 1. | `Value` (TritVal), `GetChar` ('-', '0', '+'), `NEG()`, `XOR()`, `SUM()`, `IMP()`, `AND()`, `OR()`, `MIN()`, `MAX()`, `EQUAL()`, `NOTEQUAL()`, `Positive(Action)`, `Negative(Action)`, `Zero(Action)`, `Else(Action)`, `Spaceship<T>()` extension |
| `Tryte` | 6-trit (configurable) balanced ternary byte equivalent. | `N_TRITS_PER_TRYTE` (static), `PackedTrits`, `Value` (Trit[]), `TryteString`, `ShortValue`, `ADD()`, `SUB()`, `MULT()`, `DIV()`, `MOD()`, `SHIFTLEFT()`, `SHIFTRIGHT()`, `INVERT()`, operators: `+`, `-`, `*`, `/`, `%`, `<<`, `>>`, `&`, `\|`, `^`, `~` |
| `IntT` | 32-trit balanced ternary integer backed by a packed `ulong` with word-parallel (SWAR) arithmetic and O(1) three-way comparison. | `TritCount` (32), `ToInt64()`, `Sign`, `TernaryString`, `Parse()`, `TryParse()`, `Abs()`, operators: `+`, `-`, `*`, `/`, `%`, `&`, `\|`, `^`, `~`, `<<`, `>>` |
| `FloatT` | 32-trit floating-point number with 26-trit significand and 6-trit exponent. | `TotalTritCount` (32), `SignificandTritCount` (26), `ExponentTritCount` (6), `Exponent`, `SignificandPacked`, `SignificandString`, `ToDouble()`, `FromDouble()`, `Normalize()`, `Negate()`, `ToIntT()`, `FromInt()` |
| `UTrit` | Fundamental unbalanced ternary unit representing 0, 1, or 2. | `Value` (UTritVal), `GetChar` ('0', '1', '2'), `NEG()`, `SUM()`, `SUB()`, `MULT()`, `DIV()`, `XOR()`, `XNOR()`, `IMP()`, `AND()`, `OR()`, `MIN()`, `MAX()`, `EQUAL()`, `NOTEQUAL()` |
| `UTryte` | 6-trit (configurable) unbalanced ternary byte equivalent. | `N_TRITS_PER_TRYTE` (static), `PackedTrits`, `Value` (UTrit[]), `TryteString`, `ULongValue`, `ADD()`, `SUB()`, `MULT()`, `DIV()`, `MOD()`, `SHIFTLEFT()`, `SHIFTRIGHT()`, `INVERT()`, operators: `+`, `-`, `*`, `/`, `%`, `<<`, `>>`, `&`, `\|`, `^`, `~` |
| `UIntT` | 32-trit unsigned unbalanced ternary integer. | `TritCount` (32), `ToUInt64()`, `Sign`, `TernaryString`, `Parse()`, `TryParse()`, `ToIntT()`, operators: `+`, `-`, `*`, `/`, `%`, `&`, `\|`, `^`, `~`, `<<`, `>>` |
| `UFloatT` | 32-trit non-negative unbalanced ternary floating-point number. | `TotalTritCount` (32), `SignificandTritCount` (26), `ExponentTritCount` (6), `Exponent`, `SignificandPacked`, `SignificandString`, `ToDouble()`, `FromDouble()`, `Normalize()`, `ToUIntT()`, `FromUInt()` |
| `CharT` | 6-trit (configurable) character representation. | `N_TRITS_PER_CHART`, `PackedTrits`, `Value` (Trit[]), `CharTString`, similar operators to Tryte |

### Enums

| Enum | Description | Values |
|------|-------------|--------|
| `TritVal` | Enumeration for trit values. | `n = -1`, `z = 0`, `p = 1` |
| `UTritVal` | Enumeration for unbalanced trit values. | `z = 0`, `o = 1`, `t = 2` |

### Classes

| Class | Description | Key Methods |
|-------|-------------|--------------|
| `TernarySearchTree<TValue>` | Ternary search tree for string keys with ternary comparisons. | `Put(string, TValue)`, `TryGetValue(string, out TValue)`, `Items()`, `KeysWithPrefix(string)` |
| `TernaryDecision` | Fluent API for ternary conditional execution. | `Positive(Action)`, `Zero(Action)`, `Negative(Action)`, `NonPositive(Action)`, `NonNegative(Action)`, `Else(Action)`, `Choose<T>()`, `Switch<T>()`, `From(Trit)`, `Compare<T>()` |

### Static Classes

| Class | Description | Key Methods |
|-------|-------------|--------------|
| `TernaryAlgorithms` | Sorting algorithms using ternary comparisons. | `TernaryQuicksort<T>()`, `BinaryQuicksort<T>()` |
| `OptimizedQuicksort` | Production 3-way quicksort built around the balanced-ternary comparison, plus matched binary controls for evaluation. | `TernaryQuicksort3Way()`, `BinaryQuicksort3Way()`, `BinaryQuicksort2Way()` |
| `TernaryFom` / `BinaryFom` | Frieder figure-of-merit instrumentation: per-category native-ternary instruction counts, switching activity, memory traffic, and ternary:binary relations. | `Measure()`, `Report()`, `Snapshot()`, `Take()`, `Reset()`, `Enabled` |
| `TernaryConverter` | Conversion between ternary types and .NET primitives. | `IntTToInt32()`, `IntTFromInt32()`, `FloatTToFloat()`, `FloatTFromFloat()`, `TryteToUInt8()`, `TryteFromUInt8()`, etc. (full bidirectional conversions) |
| `TernaryExtensions` | Extension methods for ternary operations. | `Spaceship<T>()` (IComparable<T>), mixed balanced/unbalanced `Spaceship()` overloads, `Ternary<T>()` (IComparable<T>) |
| `UtfT` | UTF-T encoding/decoding for ternary strings. | `EncodeCodePoint(int)`, `DecodeCodePoint()`, `EncodeString(string)`, `DecodeString()` |
| `TritPacker` | Utilities for packing/unpacking trits into binary integers. | `PackTrits(Trit[])`, `UnpackTrits(uint, int)` |
| `UTritPacker` | Utilities for packing/unpacking unbalanced trits into binary integers. | `PackTrits(UTrit[])`, `UnpackTrits(uint, int)`, `PackTrits64(UTrit[])`, `UnpackTrits64(ulong, int)` |

### Extension Methods

| Method | Description |
|--------|-------------|
| `T.Spaceship<T>(T other)` | Returns Trit comparison result (-1, 0, 1) for IComparable<T> |
| `UIntT.Spaceship(IntT other)` and `IntT.Spaceship(UIntT other)` | Compares balanced and unbalanced integer values and returns a balanced `Trit` result |
| `UTryte.Spaceship(Tryte other)` and `Tryte.Spaceship(UTryte other)` | Compares balanced and unbalanced tryte values and returns a balanced `Trit` result |
| `UFloatT.Spaceship(FloatT other)` and `FloatT.Spaceship(UFloatT other)` | Compares balanced and unbalanced floating-point values and returns a balanced `Trit` result |
| `UTrit.Spaceship(Trit other)` and `Trit.Spaceship(UTrit other)` | Compares balanced and unbalanced trit values and returns a balanced `Trit` result |
| `T.Ternary<T>(T other)` | Returns TernaryDecision for fluent ternary conditionals |

### Operators

Most ternary types support standard operators:
- Arithmetic: `+`, `-`, `*`, `/`, `%`
- Bitwise: `&`, `|`, `^`, `~`
- Shift: `<<`, `>>`
- Comparison: `==`, `!=`, `<`, `<=`, `>`, `>=`
- Conversion: implicit/explicit to/from primitives

### Key Concepts

- **Balanced Ternary**: Base-3 system using -1, 0, 1 instead of 0, 1, 2
- **Unbalanced Ternary**: Base-3 system using 0, 1, 2
- **Trit**: Single ternary digit
- **Tryte**: Group of trits (default 6)
- **Bit-packing**: Trits stored as 2 bits in binary; the balanced core uses a two's-complement digit (`00`=0, `01`=+1, `11`=−1) that is order-isomorphic to the value
- **Word-parallel (SWAR) arithmetic**: All 32 trits of an `IntT` are processed at once; three-way comparison is O(1) and addition uses the dual-rail Frieder & Luk scheme
- **Figure of Merit (FoM)**: Frieder-style evaluation relating ternary operation counts, memory traffic, and digit flips to a binary machine doing the same work
- **Unsigned Unbalanced Semantics**: `UIntT`, `UTryte`, and `UFloatT` reject negative results with `OverflowException`
- **Spaceship Operator**: Returns Trit for three-way comparison
- **Ternary Decision**: Fluent API for conditional logic based on ternary results

### License ###
The MIT License (MIT)
Copyright © 2024 Jacob Jackson <jacob@actionjdjackson.online> and Dr. ir. Steven Bos <steven.bos@usn.no>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
