using System;
using System.Diagnostics;
using System.Linq;
using stdTernary;
using L = stdTernary.Legacy;

namespace stdTernary.Eval;

/// <summary>
/// Self-contained evaluation harness (no BenchmarkDotNet, so it runs anywhere):
///   1. Correctness fuzzing of the new SWAR IntT against 64-bit reference semantics
///      and against the legacy digit-serial implementation.
///   2. Micro-benchmarks: legacy IntT vs SWAR IntT vs native long.
///   3. Quicksort evaluation: optimized ternary 3-way quicksort with the
///      Frieder figure-of-merit report against binary references.
/// </summary>
public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("stdTernary SWAR evaluation - " + DateTime.Now);
        Console.WriteLine();

        Correctness();
        MicroBenchmarks();
        QuicksortEvaluation();
    }

    // ==================================================================
    // 1. Correctness
    // ==================================================================

    private static void Correctness()
    {
        Console.WriteLine("== 1. Correctness fuzzing (SWAR IntT vs 64-bit reference vs legacy) ==");
        var rng = new XorShift(0xC0FFEE);
        long half = 926510094425920; // (3^32 - 1) / 2
        int n = 200_000;
        int shiftChecks = 0;

        for (int i = 0; i < n; i++)
        {
            long a = rng.NextInRange(half);
            long b = rng.NextInRange(half);

            var ta = new IntT(a);
            var tb = new IntT(b);
            var la = new L.IntT(a);
            var lb = new L.IntT(b);

            // Round-trip
            Check(ta.ToInt64() == a, $"roundtrip {a}");

            // Compare / sign
            Check(Math.Sign(ta.CompareTo(tb)) == Math.Sign(a.CompareTo(b)), $"cmp {a},{b}");
            Check(ta.Sign == Math.Sign(a), $"sign {a}");

            // Add / sub (reference is exact; overflow must throw in both worlds)
            CheckOp(a, b, half, () => (ta + tb).ToInt64(), (x, y) => x + y, "add");
            CheckOp(a, b, half, () => (ta - tb).ToInt64(), (x, y) => x - y, "sub");

            // Negate
            Check((-ta).ToInt64() == -a, $"neg {a}");

            // Mul (use small operands to stay mostly in range, but keep overflow cases)
            long sa = a % 100_000_000, sb = b % 100_000_000;
            var tsa = new IntT(sa); var tsb = new IntT(sb);
            CheckOp(sa, sb, half, () => (tsa * tsb).ToInt64(), (x, y) => x * y, "mul");

            // Div / mod
            if (b != 0)
            {
                Check((ta / tb).ToInt64() == a / b, $"div {a}/{b}");
                Check((ta % tb).ToInt64() == a % b, $"mod {a}%{b}");
            }

            // Shifts vs legacy semantics (truncating)
            int k = (int)(rng.Next() % 32);
            Check((ta << k).ToInt64() == (la << k).ToInt64(), $"shl {a}<<{k}");
            Check((ta >> k).ToInt64() == (la >> k).ToInt64(), $"shr {a}>>{k}");
            shiftChecks++;

            // Tritwise ops vs legacy
            Check((ta & tb).ToInt64() == (la & lb).ToInt64(), $"and {a},{b}");
            Check((ta | tb).ToInt64() == (la | lb).ToInt64(), $"or {a},{b}");
            Check((ta ^ tb).ToInt64() == (la ^ lb).ToInt64(), $"xor {a},{b}");
        }

        // Edge cases
        Check(IntT.MaxValue.ToInt64() == half, "MaxValue");
        Check(IntT.MinValue.ToInt64() == -half, "MinValue");
        Check((IntT.MaxValue + IntT.MinValue).ToInt64() == 0, "max+min");
        Check(default(IntT).ToInt64() == 0, "default(IntT) == 0");
        CheckThrows<OverflowException>(() => _ = IntT.MaxValue + IntT.One, "overflow add");
        CheckThrows<OverflowException>(() => _ = IntT.MaxValue * new IntT(2), "overflow mul");
        CheckThrows<DivideByZeroException>(() => _ = IntT.One / IntT.Zero, "div by zero");
        Check(IntT.Parse("+-0+").ToInt64() == 27 - 9 + 1, "parse");

        Console.WriteLine($"   PASS - {n:N0} random vectors x 12 properties + edge cases ({_checks:N0} checks total)");
        Console.WriteLine();
    }

    private static long _checks;

    private static void Check(bool ok, string what)
    {
        _checks++;
        if (!ok) throw new Exception("CORRECTNESS FAILURE: " + what);
    }

    private static void CheckOp(long a, long b, long half, Func<long> ternary, Func<long, long, long> reference, string name)
    {
        _checks++;
        long expected;
        bool refOverflows;
        try { expected = reference(a, b); refOverflows = Math.Abs(expected) > half; }
        catch (OverflowException) { expected = 0; refOverflows = true; }

        if (refOverflows)
        {
            try { ternary(); throw new Exception($"CORRECTNESS FAILURE: {name} {a},{b} should overflow"); }
            catch (OverflowException) { /* expected */ }
        }
        else
        {
            long got = ternary();
            if (got != expected)
                throw new Exception($"CORRECTNESS FAILURE: {name} {a},{b}: got {got}, want {expected}");
        }
    }

    private static void CheckThrows<T>(Action a, string what) where T : Exception
    {
        _checks++;
        try { a(); }
        catch (T) { return; }
        throw new Exception($"CORRECTNESS FAILURE: {what} did not throw {typeof(T).Name}");
    }

    // ==================================================================
    // 2. Micro-benchmarks
    // ==================================================================

    private static void MicroBenchmarks()
    {
        Console.WriteLine("== 2. Micro-benchmarks (ns/op, operands pre-constructed) ==");
        const int N = 4096;
        var rng = new XorShift(0xBEEF);
        long half = 926510094425920;

        var xs = new long[N]; var ys = new long[N];
        for (int i = 0; i < N; i++) { xs[i] = rng.NextInRange(half / 2); ys[i] = rng.NextInRange(half / 2) | 1; }
        var txs = xs.Select(v => new IntT(v)).ToArray();
        var tys = ys.Select(v => new IntT(v)).ToArray();
        var lxs = xs.Select(v => new L.IntT(v)).ToArray();
        var lys = ys.Select(v => new L.IntT(v)).ToArray();

        long sinkL = 0; ulong sinkU = 0; int sinkI = 0;

        Console.WriteLine($"{"operation",-14}{"legacy IntT",14}{"SWAR IntT",14}{"speedup",10}{"native long",14}{"SWAR vs native",16}");

        Row("compare",
            () => { for (int i = 0; i < N; i++) sinkI += lxs[i].CompareTo(lys[i]); },
            () => { for (int i = 0; i < N; i++) sinkI += txs[i].CompareTo(tys[i]); },
            () => { for (int i = 0; i < N; i++) sinkI += xs[i].CompareTo(ys[i]); }, N);

        Row("add",
            () => { for (int i = 0; i < N; i++) sinkU += (lxs[i] + lys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkU += (txs[i] + tys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkL += xs[i] + ys[i]; }, N);

        Row("subtract",
            () => { for (int i = 0; i < N; i++) sinkU += (lxs[i] - lys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkU += (txs[i] - tys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkL += xs[i] - ys[i]; }, N);

        Row("negate",
            () => { for (int i = 0; i < N; i++) sinkU += (-lxs[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkU += (-txs[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkL += -xs[i]; }, N);

        var mxs = xs.Select(v => v % 100_000).ToArray();
        var mys = ys.Select(v => (v % 100_000) | 1).ToArray();
        var tmxs = mxs.Select(v => new IntT(v)).ToArray();
        var tmys = mys.Select(v => new IntT(v)).ToArray();
        var lmxs = mxs.Select(v => new L.IntT(v)).ToArray();
        var lmys = mys.Select(v => new L.IntT(v)).ToArray();

        Row("multiply",
            () => { for (int i = 0; i < N; i++) sinkU += (lmxs[i] * lmys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkU += (tmxs[i] * tmys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkL += mxs[i] * mys[i]; }, N);

        Row("divide",
            () => { for (int i = 0; i < N; i++) sinkU += (lxs[i] / lys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkU += (txs[i] / tys[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkL += xs[i] / ys[i]; }, N);

        Row("shift <<3",
            () => { for (int i = 0; i < N; i++) sinkU += (lxs[i] << 3).Packed; },
            () => { for (int i = 0; i < N; i++) sinkU += (txs[i] << 3).Packed; },
            () => { for (int i = 0; i < N; i++) sinkL += xs[i] << 3; }, N);

        Row("construct",
            () => { for (int i = 0; i < N; i++) sinkU += new L.IntT(xs[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkU += new IntT(xs[i]).Packed; },
            () => { for (int i = 0; i < N; i++) sinkL += xs[i]; }, N);

        Row("to Int64",
            () => { for (int i = 0; i < N; i++) sinkL += lxs[i].ToInt64(); },
            () => { for (int i = 0; i < N; i++) sinkL += txs[i].ToInt64(); },
            () => { for (int i = 0; i < N; i++) sinkL += xs[i]; }, N);

        GC.KeepAlive(sinkL + (long)sinkU + sinkI);
        Console.WriteLine();
    }

    private static void Row(string name, Action legacy, Action swar, Action native, int opsPerCall)
    {
        double tl = TimeNsPerOp(legacy, opsPerCall);
        double ts = TimeNsPerOp(swar, opsPerCall);
        double tn = TimeNsPerOp(native, opsPerCall);
        Console.WriteLine($"{name,-14}{tl,12:F2}ns{ts,12:F2}ns{tl / ts,9:F1}x{tn,12:F2}ns{ts / Math.Max(tn, 0.01),13:F1}x");
    }

    private static double TimeNsPerOp(Action body, int opsPerCall)
    {
        // Warmup
        for (int i = 0; i < 50; i++) body();

        double best = double.MaxValue;
        for (int rep = 0; rep < 7; rep++)
        {
            int calls = 200;
            var sw = Stopwatch.StartNew();
            for (int c = 0; c < calls; c++) body();
            sw.Stop();
            double ns = sw.Elapsed.TotalMilliseconds * 1e6 / ((double)calls * opsPerCall);
            if (ns < best) best = ns;
        }
        return best;
    }

    // ==================================================================
    // 3. Quicksort evaluation
    // ==================================================================

    private static void QuicksortEvaluation()
    {
        Console.WriteLine("== 3. Optimized ternary quicksort: wall-clock + Frieder FoM ==");

        foreach (var (label, data) in Distributions(100_000))
        {
            Console.WriteLine($"---- distribution: {label}, n = {data.Length:N0} ----");

            // ---- wall clock (FoM disabled) ----
            double tRepo = TimeSort(data, a => TernaryAlgorithms.TernaryQuicksort(a.Select(v => new IntT(v)).ToArray()));
            double tOpt = TimeSort(data, a => OptimizedQuicksort.TernaryQuicksort3Way(a.Select(v => new IntT(v)).ToArray()));
            double tBin3 = TimeSort(data, a => { var c = (long[])a.Clone(); OptimizedQuicksort.BinaryQuicksort3Way(c); });
            double tBin2 = TimeSort(data, a => { var c = (long[])a.Clone(); OptimizedQuicksort.BinaryQuicksort2Way(c); });
            double tArr = TimeSort(data, a => { var c = (long[])a.Clone(); Array.Sort(c); });

            Console.WriteLine($"  wall clock: repo TernaryQuicksort {tRepo,8:F2} ms | optimized ternary 3-way {tOpt,8:F2} ms " +
                              $"| binary 3-way {tBin3,7:F2} ms | binary 2-way Hoare {tBin2,7:F2} ms | Array.Sort {tArr,7:F2} ms");

            // ---- correctness of the sorts ----
            var check = data.Select(v => new IntT(v)).ToArray();
            OptimizedQuicksort.TernaryQuicksort3Way(check);
            var sortedRef = (long[])data.Clone(); Array.Sort(sortedRef);
            for (int i = 0; i < check.Length; i++)
                if (check[i].ToInt64() != sortedRef[i]) throw new Exception("sort incorrect at " + i);

            // ---- Frieder FoM: instrumented runs ----
            TernaryFom.Reset(); BinaryFom.Reset();
            var tArrK = data.Select(v => new IntT(v)).ToArray();
            var snap = TernaryFom.Measure($"ternary 3-way quicksort / {label}",
                () => OptimizedQuicksort.TernaryQuicksort3Way(tArrK));

            var bArr3 = (long[])data.Clone();
            BinaryFom.Enabled = true;
            var b0 = BinaryFom.Take($"binary 3-way quicksort / {label}");
            OptimizedQuicksort.BinaryQuicksort3Way(bArr3);
            var bSnap = BinaryFom.Take($"binary 3-way quicksort / {label}") - b0;
            BinaryFom.Enabled = false;

            Console.WriteLine(TernaryFom.Report(snap, bSnap, data.Length));
        }
    }

    private static (string, long[])[] Distributions(int n)
    {
        var rng = new XorShift(0xF00D);
        long half = 926510094425920;

        var random = new long[n];
        for (int i = 0; i < n; i++) random[i] = rng.NextInRange(half);

        var dupHeavy = new long[n];
        var values = Enumerable.Range(0, 16).Select(_ => rng.NextInRange(half)).ToArray();
        for (int i = 0; i < n; i++) dupHeavy[i] = values[rng.Next() % 16];

        var sorted = (long[])random.Clone(); Array.Sort(sorted);
        var reversed = (long[])sorted.Clone(); Array.Reverse(reversed);

        return new[]
        {
            ("uniform random", random),
            ("duplicate-heavy (16 distinct keys)", dupHeavy),
            ("already sorted", sorted),
            ("reverse sorted", reversed),
        };
    }

    private static double TimeSort(long[] data, Action<long[]> sort)
    {
        sort(data); // warmup + JIT
        double best = double.MaxValue;
        for (int rep = 0; rep < 5; rep++)
        {
            var sw = Stopwatch.StartNew();
            sort(data);
            sw.Stop();
            best = Math.Min(best, sw.Elapsed.TotalMilliseconds);
        }
        return best;
    }

    // Simple deterministic RNG
    private sealed class XorShift
    {
        private ulong _s;
        public XorShift(ulong seed) => _s = seed == 0 ? 1 : seed;
        public ulong Next()
        {
            _s ^= _s << 13; _s ^= _s >> 7; _s ^= _s << 17;
            return _s;
        }
        public long NextInRange(long maxMagnitude)
        {
            long v = (long)(Next() % (ulong)(2 * maxMagnitude + 1)) - maxMagnitude;
            return v;
        }
    }
}
