# stdTernary #
## A standard library for Ternary operations in C# ##
![build-and-test](https://github.com/actionjdjackson/stdTernary/actions/workflows/build-and-test.yml/badge.svg?event=push)

(2do) what this library is and what it enables: an elegant ternary first high level programming experience. The library abstract the low level implementation such that in the future the same c# source code can be recompiled on a ternary computer architecture. 

The library promotes the usage of mixed radix programming, using binary for binary concepts and ternary for ternary concepts. This means that boolean is great to model concepts with only 2 logical choices (True, False) and kleenean is great to model concepts with 3 choices (True, False, Unknown). Kleene logic is clear in what happens with the unknown state when using implications.    

### Balanced and Unbalanced Ternary

stdTernary supports both balanced ternary and unbalanced ternary:

- **Balanced ternary** uses `-1`, `0`, and `1`, represented as `-`, `0`, and `+` in strings. The balanced types are `Trit`, `Tryte`, `IntT`, `FloatT`, and `TritPacker`.
- **Unbalanced ternary** uses `0`, `1`, and `2`, represented as `0`, `1`, and `2` in strings. The unbalanced types are prefixed with `U`: `UTrit`, `UTryte`, `UIntT`, `UFloatT`, and `UTritPacker`.

The unbalanced types implement ternary-native arithmetic, packing, tritwise operations, parsing, formatting, and conversions to/from the matching balanced types where the value is representable. Since unbalanced ternary is unsigned, operations that would produce a negative unbalanced value throw `OverflowException`.

### Type Conversion Library

The library now includes a comprehensive **TernaryConverter** class that provides seamless conversion between balanced ternary types and common .NET binary types:

- **IntT** ↔ int, uint, short, ushort, byte, sbyte, long, ulong, decimal
- **FloatT** ↔ float, double, decimal  
- **Tryte** ↔ byte, sbyte, short, ushort, uint, ulong, decimal
- **Trit** ↔ bool

All conversions include proper overflow checking and error handling. See [CONVERTER_README.md](CONVERTER_README.md) for detailed documentation and examples.

### Installation via NuGet
(2do) 

### Installation via Github Codespaces
- Click on the green Code button and then on the tab Codespaces. Choose "create new codespace on main".
- To build the .dll library type "task build"
- To build and run the unit tests type "task test" 
- To run the benchmark suite type "task run"

### Limitations
- The library uses binary coded ternary (BCT) using 2 bits to represent 1 trit. This is obviously inefficient as 1 of the 4 states is ignored. Balanced ternary reserves one 2-bit state because it only needs `-1`, `0`, and `1`; unbalanced ternary reserves one 2-bit state because it only needs `0`, `1`, and `2`. We define 6 trits as 1 tryte such that all binary states of a single byte can fit in a single tryte which simplifies conversions and adds binary compatibility. A tryte (6 trits implemented as 12 bits) is packed as a binary integer. Note that balanced ternary is always signed so the usage of unsigned binary storage is just a choice of implementation.

Since the current compilation targets are all binary hardware platforms, this inefficiency is unavoidable. However, we expect that this is temporary as more and more balanced ternary instructions set architectures are being developed such as REBEL-6 and ART-9 which feature ternary assembly with ternary logic, memory and branching. With the new .net 10 featuring RISCV support, a possible compilation flow from c# to REBEL-6 (32-trit balanced ternary, binary compatible ISA) is technically possible through R2R. This is work in progress. The R2R framework allows low level binary vs ternary comparison to compute fairly the amount of memory and bitflips that are needed for both radixes to run the same code. 

### Dev Notes sep 2025 (by Jackson)
I've completely overhauled stdTernary - no longer using an enum for storing +, -, and 0 values. Instead, I'm trit-packing 2-bit-trits (0b10 is -1, 0b01 is 1, and 0b00 is zero, while 0b11 is reserved/unused) into binary unsigned integers (`uint`s or `ulong`s) but all the math is still done in Ternary.

All bitwise and bytewise operators have been overriden for trits and trytes. I am currently using the `*` operator on trits for XNOR/MULTIPLY. The structs are `Trit` and `Tryte` - and the `Tryte` can be modified easily to be any number of trits you want, up to 16 trits with the current implementation. Each `Tryte` holds a bitpacked array of trits in a `uint` - using 6 by default, up to 16. All math is done in Ternary.

Includes a customizable `IntT` struct that can have any number of total trits (up to 32 - bitpacking into a 64-bit `ulong`) in its implementation. It follows the same convention as the `Tryte`, but is able to work with non-multiples of trytes - like 21-trit or 32-trit integers. All math is done in Ternary.

Also includes a customizable `FloatT` struct that can have any number of total trits - up to 32, separated into a exponent and a significand. The significand can be any number of trits - typically 26 so that the exponent has 6 to work with. It doesn't have to be a multiple of 3. The `FloatT` struct holds a combination the exponent and the significand as bitpacked trits. All math is done in Ternary.

Also includes most of the `Math` functions specifically for use with these `FloatT`s and some for use with `IntT`s in a static class called `MathT`. I also added a `Log3` function and an `ILogT` function and trit increment/decrement functions for `FloatT`s.

The `string` conversion is for interoperability with my "Action Ternary Simulator" which runs on strings of `+, -, and 0` characters and does all the math in Ternary. Also for quick visualization of the ternary values as symbols and checking the outputs of functions like `SHIFTRIGHT` or `SHIFTLEFT` or trit increment/decrement.

The library now includes unbalanced ternary equivalents for the core balanced types. These use the same 2-bit-per-trit packing strategy, but the digit set is `0`, `1`, and `2`. `UTryte` is configurable like `Tryte`, `UIntT` is a 32-trit unsigned integer, and `UFloatT` is a non-negative floating-point ternary type with an unbalanced significand and biased unbalanced exponent storage.

`Tryte` and `FloatT` and `IntT` and have modifiable static integer values which is where you can "customize" them to certain sizes.

## Detailed API Reference

### Core Types

| Type | Description | Key Properties/Methods |
|------|-------------|-------------------------|
| `Trit` | Fundamental ternary unit representing -1, 0, or 1. | `Value` (TritVal), `GetChar` ('-', '0', '+'), `NEG()`, `XOR()`, `SUM()`, `IMP()`, `AND()`, `OR()`, `MIN()`, `MAX()`, `EQUAL()`, `NOTEQUAL()`, `Positive(Action)`, `Negative(Action)`, `Zero(Action)`, `Else(Action)`, `Spaceship<T>()` extension |
| `Tryte` | 6-trit (configurable) balanced ternary byte equivalent. | `N_TRITS_PER_TRYTE` (static), `PackedTrits`, `Value` (Trit[]), `TryteString`, `ShortValue`, `ADD()`, `SUB()`, `MULT()`, `DIV()`, `MOD()`, `SHIFTLEFT()`, `SHIFTRIGHT()`, `INVERT()`, operators: `+`, `-`, `*`, `/`, `%`, `<<`, `>>`, `&`, `\|`, `^`, `~` |
| `IntT` | 32-trit arbitrary-precision integer. | `TritCount` (32), `ToInt64()`, `Sign`, `TernaryString`, `Parse()`, `TryParse()`, operators: `+`, `-`, `*`, `/`, `%`, `&`, `\|`, `^`, `~`, `<<`, `>>` |
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
- **Bit-packing**: Trits stored as 2 bits in binary for efficiency
- **Unsigned Unbalanced Semantics**: `UIntT`, `UTryte`, and `UFloatT` reject negative results with `OverflowException`
- **Spaceship Operator**: Returns Trit for three-way comparison
- **Ternary Decision**: Fluent API for conditional logic based on ternary results


### License ###
The MIT License (MIT)
Copyright © 2024 Jacob Jackson <jacob@actionjdjackson.online> and Dr. ir. Steven Bos <steven.bos@usn.no>

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

## Frieder-style evaluation: how would your code run on native ternary hardware?

Frieder & Luk (1973) argued that a non-binary architecture emulated on a binary
host should not be judged by wall-clock speed against the host — "there is no
point whatsoever to do any speed comparisons" — but by **monitoring operation
counts per category, memory utilization, and relating them to a binary machine
doing the same work**. `TernaryFom` implements exactly that: every `IntT`
operation is metered, and one counter unit corresponds to one instruction on a
hypothetical balanced-ternary register machine (a full `<,=,>` comparison is a
*single* instruction there).

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

The report prints the ternary instruction mix (comparisons, add/sub, mul, div,
negate, shift, tritwise), switching activity (trit flips), memory traffic in
trits, and the ternary:binary relations for comparisons, control operations and
digit flips.

## Optimized 3-way quicksort

`OptimizedQuicksort.TernaryQuicksort3Way` is a production-style quicksort built
around the balanced-ternary comparison: a Dutch-national-flag 3-way partition
driven by **one spaceship comparison per element** (the single trit routes each
element to the less/equal/greater region), randomized median-of-three pivots,
an insertion-sort cutoff, and recursion on the smaller side only. Duplicate
keys collapse into the pivot block, giving O(n · log(distinct)) behaviour.
Binary controls (`BinaryQuicksort3Way`, `BinaryQuicksort2Way`) with the same
structure are provided so FoM relations are apples-to-apples.

## SWAR packed representation (performance)

The packed encoding stores each trit as a 2-bit two's-complement digit
(`00`=0, `01`=+1, `11`=−1). Flipping each field's sign bit makes the packed
word order-isomorphic to the numeric value, so a 3-way compare is one XOR and
one unsigned compare. The low bits form the "occupied" rail and the high bits
the "negative" rail, giving the dual-rail form used by Frieder & Luk for
word-parallel addition: all 32 trits are added at once with Boolean operations
and carries ripple by whole words. Negation is a rail swap; ternary shifts are
single machine shifts; multiply/divide use fast 6-trit-limb table conversions
and the host multiplier. Run `stdTernary.Eval` for correctness fuzzing and the
full benchmark/FoM suite.
