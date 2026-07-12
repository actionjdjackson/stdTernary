## Motivation

The library's arithmetic was trit-serial: every `+` decoded 32 trits into bytes, ran a branchy carry loop, and re-encoded — hundreds of dependent operations per add — and `Compare` was a 32-iteration digit loop, which contradicts the balanced-ternary thesis that a full `<,=,>` comparison is one instruction. This PR makes the packed word itself do the work, and adds the evaluation methodology of **Frieder & Luk (1973, _Ternary Computers: Part 1/2_)**: judge an emulated ternary machine by **operation counts per category, memory utilization, and relations to a binary machine doing the same work**, not by wall-clock speed against the binary host.

The result is concrete evidence of a **clear ternary advantage on several figures of merit**.

## What changed

### 1. Encoding (`BalancedTernaryEncoding.cs`) — breaking internal layout
Each trit is now a 2-bit **two's-complement digit**: `00`=0, `01`=+1, `11`=−1.
- `default` structs still encode zero (`packed == 0`).
- XOR-ing each field's sign bit makes the packed word **order-isomorphic** to the value → 3-way compare = 1 XOR + 1 unsigned compare (O(1), branch-predictable).
- Low bits = "occupied" rail, high bits = "negative" rail → the **dual-rail form of Frieder & Luk** falls out with two masks. Word-parallel addition runs tritwise half-adder rounds over the rails; carries ripple by whole words (typically 2–4 rounds). Transient carries out of trit 31 are accumulated in a scalar "trit 32" because in balanced ternary they can legitimately cancel (`+-` + `+-` = `++`); overflow is decided only after carries settle.
- Negation = rail swap (O(1)). Ternary shifts = single machine shifts. Fast `Int64`↔ternary via 6-trit (balanced base-729) limb tables. Multiply/divide go through the checked 64-bit fast path (32-trit products always fit an in-range `long`), raising `OverflowException` on out-of-range results exactly as before.

**Compatibility:** `Tryte`, `CharT`, `FloatT`, … access the packed form only through `BalancedTernaryEncoding`, so they keep working unchanged (and their compares get O(1) for free). **All 93 existing tests pass.** `TritPacker` has its own separate layout and is untouched.

### 2. Frieder FoM instrumentation (`TernaryFom.cs`)
Wrap **any C# region** for a native-hardware summary:

```csharp
var snap = TernaryFom.Measure("kernel", () => { /* any IntT-using code */ });
Console.WriteLine(TernaryFom.Report(snap, binarySnapshot, n));
```

Counters (1 unit = 1 hypothetical native ternary instruction): 3-way comparisons, add/sub, mul, div, negate, shift, tritwise ops; plus switching activity (trit flips), memory traffic (trits written), and data memory in trits vs bits. `BinaryFom` meters a binary reference implementation so the report prints the ternary:binary **relations** Frieder proposed. Hooks live behind a static `Enabled` flag (one predictable branch when off).

### 3. Optimized quicksort (`OptimizedQuicksort.cs`)
Dutch-national-flag 3-way partition driven by **one spaceship comparison per element**, randomized median-of-three pivots, insertion cutoff, smaller-side recursion. Binary controls (`BinaryQuicksort3Way`, `BinaryQuicksort2Way`) with identical structure for apples-to-apples relations.

### 4. Evaluation harness (`stdTernary.Eval/`)
Dependency-free console project (no BenchmarkDotNet/nuget): 200k-vector correctness fuzz vs 64-bit reference semantics **and** vs a bundled legacy digit-serial `IntT` (2.8M checks, all passing), Stopwatch micro-benchmarks, and the quicksort FoM study. Targets `net10.0`, wired into the solution.

## Results — evidence of ternary advantage

### Quicksort FoM relations (ternary : binary 3-way, n = 100,000, deterministic counters)

| distribution | comparison instructions | total control ops | digit flips (trit:bit) |
|---|---|---|---|
| uniform random | **0.69** | 0.79 | 0.83 |
| duplicate-heavy (16 keys) | **0.62** | 0.74 | 0.80 |
| already sorted | **0.77** | 0.86 | 0.97 |
| reverse sorted | **0.71** | 0.79 | 0.84 |

On native ternary hardware the same algorithm executes **~30–40% fewer comparison instructions** (a binary ISA pays 1–2 compares to recover each 3-way decision), with duplicate-heavy inputs benefiting most, moving ~3.2M trits (~5.07M bit-equivalents) instead of 6.4M bits of key storage.

### Micro-benchmarks (ns/op, SWAR vs legacy)
add **17.7×**, subtract **24.0×**, multiply **23.7×**, divide **14.1×**, shift **17.3×** faster than the legacy digit-serial implementation. (Host wall-clock is reported for transparency; per Frieder the FoM relations above are the architecture-relevant result.)

## How to run

```bash
cd stdTernary.Eval && dotnet run -c Release
```

## Notes for reviewers
- `global.json` is left at SDK 10 and the Eval project targets `net10.0` (this is what the repo already uses). An SDK-8-only CI box would need `global.json` relaxed to `rollForward: latestMajor`.
- Breaking change is limited to the **internal** packed layout; any test asserting raw `Packed` values (none currently) would need updating.

🤖 Generated with [Claude Code](https://claude.com/claude-code)
