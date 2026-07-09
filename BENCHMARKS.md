# Benchmark Findings

Notes from investigating why unbalanced ternary appeared to dramatically outperform
balanced ternary and binary in the arithmetic benchmarks. Numbers are from an Apple
M4 Pro on .NET 10 and are relative; treat the ratios, not the absolute ns.

## TL;DR

The original "unbalanced ternary beats everything" result was **three implementation
artifacts**, not a property of the number systems. After fixing them, the three
software types land within ~2x of each other, and all remain ~100-400x slower than a
native machine `add`. `IntT`/`IntB`/`UIntT` all *simulate* arithmetic with a per-digit
software loop over a `ulong`; none of this measures ternary-vs-binary on real hardware.

## Root causes

1. **Balanced's per-digit value-translation (the ~4.6x headline).**
   Unbalanced trits `{0,1,2}` are stored as their literal 2-bit value, so decode is a
   bare cast and encode a bare store. Balanced trits `{-1,0,+1}` were stored as
   `{00,01,10}` and decoded/encoded through a `switch`, plus carry normalization used
   data-dependent `while` loops. ~1.6-1.8x per digit, compounded over dozens of
   decode/encode calls per operation.
   *Fix:* branchless decode/encode over the same layout
   (`trit = bits - 3*(bits>>1)`, `code = v + (3 & (v>>31))`). Same stored bytes, faster.

2. **Binary's per-bit integer division.**
   `BinaryEncoding.DecodeBit` did `packed / 2^index` then `% 2` — a 64-bit division
   *per bit* — where the ternary types use one shift+mask. ~4.6x slower per digit, over
   64 bits. *Fix:* `(packed >> index) & 1` (and shift/or in `SetBit`/`Encode`).

3. **The framing.** Real "binary on binary hardware" is one instruction. A native
   `long a+b` measures **~0.67 ns**; the software `IntB` add is ~250 ns (~380x). No fair
   version of these benchmarks can show ternary beating binary on binary hardware.
   *Fix:* added native `int`/`long` baselines to the suite so the report says so.

## Results (isolated, after fixes)

| Operation (per op)        | Balanced | Unbalanced | Binary (sw) | Native |
|---------------------------|---------:|-----------:|------------:|-------:|
| addition (operands ready) |  79 ns   |   75 ns    |   202 ns    | 0.67 ns |
| construction (int→type)   |  47 ns   |   29 ns    |    29 ns    |   –    |
| combined construct+add    | 186 ns   |  132 ns    |   252 ns    |   –    |

Balanced-vs-unbalanced *arithmetic* is now **1.06x** (parity); it was 3.8x. The binary
bit-decode went 98.8 ns → 15.3 ns and is now faster per-digit than a ternary trit.

## Is `IntB` a fair comparison? (keep it, but as a labeled control)

`IntB` is a *software simulation* of binary — sign byte + 64-bit magnitude, decoded a
bit at a time — exactly parallel to how `IntT`/`UIntT` simulate ternary over a `ulong`.
It is **not** "binary on binary hardware": that is the native `int`/`long` baseline
(~0.45 µs/100 vs `IntB`'s ~26 µs — a ~40-130x gap). `IntB`'s only legitimate role is a
same-methodology control: it shows the digit-loop overhead is generic to
simulation + digit width, not specific to ternary.

It is **not "between" balanced and unbalanced** in either regime:
- *On binary hardware (this benchmark):* `IntB` is the slowest (add: UIntT 13.4 <
  IntT 20.7 < IntB 26.1 µs) because it processes 64 bit-slots vs 32 trit-slots — a
  width artifact, not "binary is slow." A range-matched ~51-bit binary would narrow it.
- *On hypothetical ternary hardware:* binary is the foreign representation — you waste
  one of three states per trit, or pay binary↔ternary conversion — so it would be the
  **worst** of the three, not in between. Balanced and unbalanced are the two
  native-to-ternary options; binary is the outsider.

Takeaway: rely on the native `int`/`long` baselines as the real binary reference; keep
`IntB` only as an explicitly-labeled "software binary, same method" control.

## Why balanced is still ~1.6x on construction (and it's fine)

The residual is entirely in **decimal/binary → balanced conversion**, not arithmetic.
Decomposition of the conversion's digit-extraction loop:

| digit extraction (no encode)          | ns    | vs unbalanced |
|---------------------------------------|------:|--------------:|
| unbalanced (1 divmod, no carry-fold)  | 10.2  | 1.00x |
| balanced (1 divmod + branch fold)     | 25.6  | 2.52x |
| balanced (branchless `s = r/2`)       | 28.2  | 2.77x |
| balanced (naive 2-modulo branchless)  | 57.2  | 5.61x |

Findings:
- **Not "binary favors unsigned":** native signed vs unsigned `x/3 + x%3` is **1.00x**.
- **Not the branch:** a branchless carry-fold is no faster.
- **It's the carry feedback.** Unbalanced base-3 extraction has none — `digit = n%3` is a
  side output, `n = n/3` is independent, so the serial critical path is just the divide.
  Balanced must fold each digit's carry back into the running quotient
  (`n = (n - d)/3`), lengthening the dependency chain of an inherently serial ~13-step
  loop. This is a **binary↔ternary boundary cost**: it exists only because operands
  start as binary integers. On native ternary hardware the values are already trits and
  there is no conversion.
- **The number-system intuition is correct but hardware-only.** Balanced genuinely
  produces ~40% fewer carry events per addition (measured **3.36 vs 5.72** for
  unbalanced) because positive and negative carries cancel. But the software loop
  processes all 32 positions unconditionally at fixed cost, so carry frequency changes
  *ripple-carry delay in silicon*, not instructions retired in software.

## Other fixes

- **Tryte NA benchmarks** were benchmark bugs, not type bugs: `TestTryteArithmetic`
  multiplied operands whose product (±400) overflowed the 6-trit ±364 range;
  `TestTryteBitwise`/`TestUTryteBitwise` shifted full-range values whose high trits were
  dropped. Operand ranges were corrected.
- **`<<` now truncates** (drops trits shifted past the top) instead of throwing
  `OverflowException`, matching hardware shift semantics and `IntB`.

## Running the benchmarks

The full default run is ~40 min because the class-level `[Params(10,100,1000)]` runs all
52 benchmarks 3x (only 4 use `DataSize`), and two diagnosers each add a pass. For a fast,
still-good run:

```
# targeted subset in seconds:
dotnet run -c Release --project stdTernary -- --filter '*Addition*' --job short

# whole suite, ~4-5 min (the '*' filter is required; no args = interactive menu):
dotnet run -c Release --project stdTernary -- --filter '*' --job short

# add allocation columns (roughly doubles the time):
dotnet run -c Release --project stdTernary -- --filter '*' --job short --memory
```

`--job short` uses fewer warmup/measurement iterations (higher variance, same ranking);
`--filter` runs a glob subset. `DataSize` scaling now only applies to the 4 benchmarks
that use it, so nothing runs 3x redundantly. For a publishable number on one comparison,
filter to it and drop `--job short` for full fidelity (still ~2 min for a handful).
