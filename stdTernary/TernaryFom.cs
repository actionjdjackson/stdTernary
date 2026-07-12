using System;
using System.Text;

namespace stdTernary;

/// <summary>
/// Frieder-style evaluation counters ("figures of merit", FoM).
///
/// Frieder (1973) argued that emulated non-binary machines should NOT be judged by
/// wall-clock speed on the binary host - "there is no point whatsoever to do any
/// speed comparisons" - but by monitoring the types of operations performed, the
/// number of instructions used in each category, and memory utilization, and then
/// relating those numbers to a binary machine performing the same type of work.
///
/// This class implements that methodology for stdTernary. Every metered IntT
/// operation increments a category counter while <see cref="Enabled"/> is true.
/// Each counter unit corresponds to ONE native instruction on a hypothetical
/// balanced-ternary register machine (a 3-way compare is one instruction there).
/// Wrap any code region with <see cref="Measure(string, Action)"/> to get a
/// per-region snapshot, and call <see cref="Report"/> for the hardware summary.
///
/// Counted figures of merit:
///  - instruction mix: comparisons, add/sub, mul, div, negations, shifts, tritwise ops
///  - dynamic switching activity: trit flips on recorded stores (RecordWrite/RecordSwap)
///  - memory traffic: trits written
/// The binary side of the comparison is collected by <see cref="BinaryFom"/> from a
/// reference implementation doing the same work, exactly as Frieder proposed.
/// </summary>
public static class TernaryFom
{
    public static bool Enabled;

    public static long Comparisons;
    public static long Additions;      // includes subtractions (same unit on hardware)
    public static long Multiplications;
    public static long Divisions;
    public static long Negations;
    public static long Shifts;
    public static long TritwiseOps;    // MIN / MAX / XOR
    public static long Swaps;
    public static long TritFlips;      // trits that actually changed on recorded stores
    public static long TritsWritten;   // total trits stored (memory traffic)

    public static long TotalInstructions =>
        Comparisons + Additions + Multiplications + Divisions + Negations + Shifts + TritwiseOps;

    public static void Reset()
    {
        Comparisons = Additions = Multiplications = Divisions = 0;
        Negations = Shifts = TritwiseOps = Swaps = TritFlips = TritsWritten = 0;
    }

    /// <summary>Record a store of <paramref name="newValue"/> over <paramref name="oldValue"/> (switching activity + traffic).</summary>
    public static void RecordWrite(IntT oldValue, IntT newValue)
    {
        if (!Enabled) return;
        TritFlips += BalancedTernaryEncoding.DiffTritCount(oldValue.Packed, newValue.Packed);
        TritsWritten += IntT.TritCount;
    }

    /// <summary>Record an in-place swap of two IntT storage cells.</summary>
    public static void RecordSwap(IntT a, IntT b)
    {
        if (!Enabled) return;
        Swaps++;
        int diff = BalancedTernaryEncoding.DiffTritCount(a.Packed, b.Packed);
        TritFlips += 2 * diff;              // both cells change by the same trit pattern
        TritsWritten += 2 * IntT.TritCount;
    }

    /// <summary>Run <paramref name="body"/> with counting enabled and return the delta snapshot.</summary>
    public static FomSnapshot Measure(string label, Action body)
    {
        bool was = Enabled;
        var before = Snapshot(label);
        Enabled = true;
        try { body(); }
        finally { Enabled = was; }
        return Snapshot(label) - before;
    }

    public static FomSnapshot Snapshot(string label) => new(
        label, Comparisons, Additions, Multiplications, Divisions,
        Negations, Shifts, TritwiseOps, Swaps, TritFlips, TritsWritten);

    /// <summary>
    /// Quick summary of how the measured region would run on native balanced-ternary
    /// hardware, optionally against a binary reference doing the same work.
    /// </summary>
    public static string Report(FomSnapshot t, BinaryFom.Snapshot? binary = null, int elementCount = 0)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"=== Frieder FoM report: {t.Label} ===");
        sb.AppendLine("Native-ternary instruction mix (1 counter unit = 1 hypothetical ternary instruction):");
        sb.AppendLine($"  3-way comparisons : {t.Comparisons,12:N0}");
        sb.AppendLine($"  add/sub           : {t.Additions,12:N0}");
        sb.AppendLine($"  multiply          : {t.Multiplications,12:N0}");
        sb.AppendLine($"  divide            : {t.Divisions,12:N0}");
        sb.AppendLine($"  negate            : {t.Negations,12:N0}");
        sb.AppendLine($"  shift (x3^k)      : {t.Shifts,12:N0}");
        sb.AppendLine($"  tritwise min/max/x: {t.TritwiseOps,12:N0}");
        sb.AppendLine($"  TOTAL             : {t.TotalInstructions,12:N0}");
        sb.AppendLine($"Switching activity  : {t.TritFlips:N0} trit flips over {t.TritsWritten:N0} trits written");
        if (elementCount > 0)
        {
            long tritMem = (long)elementCount * IntT.TritCount;
            double bitEquivalent = tritMem * Math.Log2(3.0);
            sb.AppendLine($"Data memory         : {tritMem:N0} trits for {elementCount:N0} elements " +
                          $"(~{bitEquivalent:N0} bit-equivalents of information; a binary long[] uses {(long)elementCount * 64:N0} bits)");
        }

        if (binary is { } b)
        {
            sb.AppendLine($"--- Binary reference doing the same work ({b.Label}) ---");
            sb.AppendLine($"  2-way comparisons : {b.Comparisons,12:N0}");
            sb.AppendLine($"  branches          : {b.Branches,12:N0}");
            sb.AppendLine($"  add/sub           : {b.Additions,12:N0}");
            sb.AppendLine($"  swaps             : {b.Swaps,12:N0}");
            sb.AppendLine($"  bit flips         : {b.BitFlips,12:N0} over {b.BitsWritten:N0} bits written");
            long bInstr = b.Comparisons + b.Branches + b.Additions;
            sb.AppendLine("--- Relations (ternary : binary), per Frieder's evaluation method ---");
            if (b.Comparisons > 0)
                sb.AppendLine($"  comparison instructions : {Ratio(t.Comparisons, b.Comparisons)} " +
                              $"(each ternary compare resolves <,=,> at once; binary needs cmp+branch pairs)");
            if (bInstr > 0)
                sb.AppendLine($"  total control ops       : {Ratio(t.TotalInstructions + t.Swaps, bInstr + b.Swaps)}");
            if (b.BitFlips > 0 && t.TritFlips > 0)
                sb.AppendLine($"  digit flips (trit:bit)  : {Ratio(t.TritFlips, b.BitFlips)} " +
                              $"(1 trit carries log2(3) = 1.585 bits of information)");
        }

        return sb.ToString();
    }

    private static string Ratio(long a, long b)
        => b == 0 ? "n/a" : $"{a:N0} : {b:N0}  =  {(double)a / b:F3}";

    public readonly record struct FomSnapshot(
        string Label, long Comparisons, long Additions, long Multiplications, long Divisions,
        long Negations, long Shifts, long TritwiseOps, long Swaps, long TritFlips, long TritsWritten)
    {
        public long TotalInstructions =>
            Comparisons + Additions + Multiplications + Divisions + Negations + Shifts + TritwiseOps;

        public static FomSnapshot operator -(FomSnapshot a, FomSnapshot b) => new(
            a.Label,
            a.Comparisons - b.Comparisons, a.Additions - b.Additions,
            a.Multiplications - b.Multiplications, a.Divisions - b.Divisions,
            a.Negations - b.Negations, a.Shifts - b.Shifts, a.TritwiseOps - b.TritwiseOps,
            a.Swaps - b.Swaps, a.TritFlips - b.TritFlips, a.TritsWritten - b.TritsWritten);
    }
}

/// <summary>
/// Counters for the binary reference implementation, so ternary and binary runs of
/// the same algorithm can be related, as in Frieder's evaluation methodology.
/// A 2-way comparison ("a &lt; b") counts one comparison; extracting a 3-way decision
/// on binary hardware typically costs one compare plus conditional branches, which
/// the reference algorithms count under <see cref="Branches"/>.
/// </summary>
public static class BinaryFom
{
    public static bool Enabled;

    public static long Comparisons;
    public static long Branches;
    public static long Additions;
    public static long Swaps;
    public static long BitFlips;
    public static long BitsWritten;

    public static void Reset()
    {
        Comparisons = Branches = Additions = Swaps = BitFlips = BitsWritten = 0;
    }

    public static void RecordSwap(long a, long b)
    {
        if (!Enabled) return;
        Swaps++;
        int diff = System.Numerics.BitOperations.PopCount((ulong)(a ^ b));
        BitFlips += 2 * diff;
        BitsWritten += 2 * 64;
    }

    public static Snapshot Take(string label)
        => new(label, Comparisons, Branches, Additions, Swaps, BitFlips, BitsWritten);

    public readonly record struct Snapshot(
        string Label, long Comparisons, long Branches, long Additions,
        long Swaps, long BitFlips, long BitsWritten)
    {
        public static Snapshot operator -(Snapshot a, Snapshot b) => new(
            a.Label,
            a.Comparisons - b.Comparisons, a.Branches - b.Branches,
            a.Additions - b.Additions, a.Swaps - b.Swaps,
            a.BitFlips - b.BitFlips, a.BitsWritten - b.BitsWritten);
    }
}
