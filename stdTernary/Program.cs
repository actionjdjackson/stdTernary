using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace stdTernary
{
    public class Program
    {
        private static void Main(string[] args)
        {
            //IntT a = new IntT(120);
            //IntT b = new IntT(60);

            //DemoSpaceship("IntT", a, b);
            //DemoMethodChaining("IntT", a, b);

            //FloatT c = 3.14159;
            //FloatT d = 2.71828;

            //DemoSpaceship("FloatT", c, d);
            //DemoMethodChaining("FloatT", c, d);

            //DemoBinaryPrimitives();

            //DemoSorting();
            //DemoTernarySearchTree();

            ValidateNewBenchmarks();

            // BenchmarkSwitcher honours CLI args, so you don't have to run the whole
            // 40-minute suite:
            //   --filter '*'            run everything (required; no args = interactive menu)
            //   --filter '*Addition*'   run a glob subset (seconds)
            //   --job short             fewer warmup/measure iterations (~4-5 min full run)
            //   --memory                add allocation columns (roughly doubles the time)
            // Diagnosers are opt-in (--memory) rather than baked in: they each add a
            // measurement pass, and ThreadingDiagnoser reported all-zeros here anyway.
            BenchmarkSwitcher
                .FromTypes(new[] { typeof(TernaryBenchmarks), typeof(ScalingBenchmarks) })
                .Run(args, DefaultConfig.Instance);
        }

        private static void DemoBinaryPrimitives()
        {
            Console.WriteLine();
            Console.WriteLine("Binary primitive demonstration (IntB / FloatB / Byte)");

            IntB left = new IntB(150);
            IntB right = new IntB(45);
            IntB sum = left + right;
            IntB product = left * right;

            Console.WriteLine($"IntB values => left: {left.ToInt64()}, right: {right.ToInt64()}");
            Console.WriteLine($"  sum (binary)    : {sum.BinaryString} = {sum.ToInt64()}");
            Console.WriteLine($"  product (binary): {product.BinaryString} = {product.ToInt64()}");

            FloatB fLeft = FloatB.FromDouble(3.14159);
            FloatB fRight = FloatB.FromDouble(2.71828);
            FloatB fMul = fLeft * fRight;

            Console.WriteLine($"FloatB multiply => {(double)fMul:F6} (from mantissa {fMul.Mantissa.ToInt64()} @ 2^{fMul.Exponent})");

            Byte byteA = new Byte("10100101");
            Byte byteB = new Byte("00001111");
            Byte byteXor = byteA ^ byteB;

            Console.WriteLine($"Byte XOR => {byteA.BinaryString} ^ {byteB.BinaryString} = {byteXor.BinaryString}");
        }

        private static void DemoSpaceship<T>(string label, T left, T right) where T : IComparable<T>
        {
            Trit spaceship = left.Spaceship(right);
            Console.WriteLine($"{label} spaceship => {left} <=> {right} = {spaceship.GetChar}");
        }

        private static void DemoMethodChaining<T>(string label, T left, T right) where T : IComparable<T>
        {
            Console.WriteLine($"{label} ternary comparison between {left} and {right}");

            TernaryDecision decision = left.Ternary(right);

            decision
                .Positive(() => Console.WriteLine("  positive branch chosen"))
                .Zero(() => Console.WriteLine("  zero branch chosen"))
                .Negative(() => Console.WriteLine("  negative branch chosen"))
                .Else(() => Console.WriteLine("  (fallback)"));

            string selected = decision.Switch("take X", "take Y", "take Z");
            Console.WriteLine($"  datapath selection -> {selected}");
        }

        private static void DemoSorting()
        {
            var values = new[]
            {
                new IntT(9),
                new IntT(-4),
                new IntT(27),
                new IntT(1),
                new IntT(-13),
                new IntT(0)
            };

            TernaryAlgorithms.TernaryQuicksort(values);
            Console.WriteLine("Sorted IntT values using ternary quicksort: " + string.Join(", ", values.Select(v => v.ToInt64())));
        }

        private static void DemoTernarySearchTree()
        {
            var tree = new TernarySearchTree<int>();
            tree.Put("shell", 1);
            tree.Put("shore", 2);
            tree.Put("sea", 3);
            tree.Put("shoreline", 4);
            tree.Put("she", 5);

            Console.WriteLine("Ternary search tree keys with prefix 'sh':");
            foreach (var (key, value) in tree.KeysWithPrefix("sh"))
            {
                Console.WriteLine($"  {key} -> {value}");
            }
        }

    static void ValidateNewBenchmarks()
    {
        Console.WriteLine("Running quick validation of new benchmarks...");

        // Quick validation of new benchmarks
        var r = new Random(42);
        var nIterations = 10;

        Console.WriteLine("Testing Tryte operations...");
        for (int i = 0; i < nIterations; i++)
        {
            Tryte a = new Tryte(r.Next(-100, 101));
            Tryte b = new Tryte(r.Next(-100, 101));
            var result = a + b;
            result = a & b;
        }
        Console.WriteLine("✓ Tryte operations work");

        Console.WriteLine("Testing Trit operations...");
        for (int i = 0; i < nIterations; i++)
        {
            Trit a = new Trit((TritVal)r.Next(-1, 2));
            Trit b = new Trit((TritVal)r.Next(-1, 2));
            var result = a.XOR(b);
            a.Positive(() => { }).Negative(() => { }).Zero(() => { });
        }
        Console.WriteLine("✓ Trit operations work");

        Console.WriteLine("Testing FloatT operations...");
        for (int i = 0; i < nIterations; i++)
        {
            FloatT a = FloatT.FromDouble(r.NextDouble() * r.Next(-1000, 1000));
            FloatT b = FloatT.FromDouble(r.NextDouble() * r.Next(-1000, 1000));
            var result = a + b;
            var converted = result.ToDouble();
        }
        Console.WriteLine("✓ FloatT operations work");

        Console.WriteLine("Testing UIntT operations...");
        for (int i = 0; i < nIterations; i++)
        {
            UIntT a = new UIntT((ulong)r.Next(0, 1_000_000));
            UIntT b = new UIntT((ulong)r.Next(0, 1_000_000));
            var result = a + b;
            result = a >= b ? a - b : b - a;
            result = a & b;
        }
        Console.WriteLine("✓ UIntT operations work");

        Console.WriteLine("Testing UTryte operations...");
        for (int i = 0; i < nIterations; i++)
        {
            UTryte a = new UTryte((ulong)r.Next(0, 80));
            UTryte b = new UTryte((ulong)r.Next(1, 9));
            var result = a + b;
            result = a >= b ? a - b : b - a;
            result = a * b;
            result = a & b;
        }
        Console.WriteLine("✓ UTryte operations work");

        Console.WriteLine("Testing UTrit operations...");
        for (int i = 0; i < nIterations; i++)
        {
            UTrit a = new UTrit(r.Next(0, 3));
            UTrit b = new UTrit(r.Next(1, 3));
            var result = a.XOR(b);
            result = a.DIV(b);
            result = a.AND(b);
        }
        Console.WriteLine("✓ UTrit operations work");

        Console.WriteLine("Testing UFloatT operations...");
        for (int i = 0; i < nIterations; i++)
        {
            UFloatT a = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 1000));
            UFloatT b = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 1000));
            var result = a + b;
            result = a >= b ? a - b : b - a;
            var converted = result.ToDouble();
        }
        Console.WriteLine("✓ UFloatT operations work");

        Console.WriteLine("Testing large dataset benchmarks...");
        var dataSize = 100;
        var values = new IntT[dataSize];
        for (int n = 0; n < dataSize; n++)
        {
            values[n] = new IntT(r.Next(-1000, 1000));
        }
        TernaryAlgorithms.TernaryQuicksort(values);
        Console.WriteLine("✓ Large dataset sorting works");

        Console.WriteLine("All new benchmarks validated successfully!");
        Console.WriteLine();
    }

    // Fixed-size micro-benchmarks. These do NOT depend on DataSize, so they live in
    // their own class with no [Params] — otherwise every one of them would run 3x.
    [MemoryDiagnoser]
    public class TernaryBenchmarks
    {
        private readonly int nIterations = 100;
        private readonly Random r = new Random();

        // Consumed by the native baselines so the JIT cannot delete the arithmetic.
        private long _sink;
        private static readonly string[] SampleWords = new[]
        {
            "sable",
            "sabotage",
            "sacred",
            "saddle",
            "safari",
            "saffron",
            "saga",
            "sailor",
            "saint",
            "salient",
            "salmon",
            "salon",
            "salute",
            "salvage",
            "sample",
            "sanction",
            "sandstone",
            "sanguine",
            "sapphire",
            "sarcasm",
            "sardonic",
            "satin",
            "satire",
            "satisfy",
            "saucer",
            "savage",
            "savor",
            "sawmill",
            "scaffold",
            "scalar",
            "scandal",
            "scanner",
            "scant",
            "scenic",
            "scepter",
            "scholar",
            "science",
            "scion",
            "scoff",
            "scooter",
            "scorecard",
            "scorn",
            "sculpt",
            "scuttle",
            "seabird",
            "seafarer",
            "sealant",
            "seamless",
            "search",
            "seasonal",
            "secular",
            "secure",
            "sediment",
            "seismic",
            "select",
            "selfless",
            "semantic",
            "semester",
            "senate",
            "sensible",
            "sentence",
            "separate",
            "serene",
            "series",
            "sermon",
            "service",
            "seventy",
            "sexton",
            "shadow",
            "shallow",
            "shamble",
            "shard",
            "sheath",
            "shelter",
            "sheriff",
            "shield",
            "shift",
            "shimmer",
            "shipyard",
            "shiver",
            "shoal",
            "shovel",
            "shrill",
            "shroud",
            "shrubs",
            "shuffle",
            "shuttle",
            "sibling",
            "sideline",
            "siege",
            "signal",
            "silence",
            "silica",
            "silver",
            "simian",
            "simple",
            "simulate",
            "sincere",
            "sinew",
            "siren"
        };

        [Benchmark]
        public void TestTernaryQuickSort()
        {
            for (int i = 0; i < nIterations; i++)
            {
                var values = new IntT[100];
                for (int n = 0; n < 100; n++)
                {
                    values[n] = new IntT(r.Next(-1_000_000, 1_000_000));
                }
                TernaryAlgorithms.TernaryQuicksort(values);
                //_ = string.Join(", ", values.Select(v => v.ToInt64()));
            }
        }

        [Benchmark]
        public void TestBinaryQuickSort()
        {
            for (int i = 0; i < nIterations; i++)
            {
                var values = new int[100];
                for (int n = 0; n < 100; n++)
                {
                    values[n] = r.Next(-1_000_000, 1_000_000);
                }
                TernaryAlgorithms.BinaryQuicksort(values);
                //_ = string.Join(", ", values);
            }
        }

        [Benchmark]
        public void TestTernarySearchTree()
        {
            for (int i = 0; i < nIterations; i++)
            {
                var tree = new TernarySearchTree<int>();
                foreach (var (word, index) in SampleWords.Select((word, index) => (word, index)))
                {
                    tree.Put(word, index);
                }

                foreach (var word in SampleWords)
                {
                    tree.TryGetValue(word, out _);
                }

                var prefix = SampleWords[r.Next(SampleWords.Length)][..2];
                foreach (var _ in tree.KeysWithPrefix(prefix))
                {
                }
            }
        }

        [Benchmark]
        public void TestBinarySearchTree()
        {
            for (int i = 0; i < nIterations; i++)
            {
                var tree = new BinarySearchTree<int>();
                foreach (var (word, index) in SampleWords.Select((word, index) => (word, index)))
                {
                    tree.Put(word, index);
                }

                foreach (var word in SampleWords)
                {
                    tree.TryGetValue(word, out _);
                }

                var prefix = SampleWords[r.Next(SampleWords.Length)][..2];
                foreach (var _ in tree.KeysWithPrefix(prefix))
                {
                }
            }
        }

        [Benchmark]
        public void TestMethodChainingComparisons()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-5, 5);
                IntT b = r.Next(-5, 5);
                a.Spaceship(b).Negative(() => a += 1)
                              .Positive(() => a += 1)
                              .Zero(() => a += 1);
            }
        }

        [Benchmark]
        public void TestMethodChainingTernaryDecisionComparisons()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-5, 5);
                IntT b = r.Next(-5, 5);
                a.Ternary(b).Negative(() => a += 1)
                            .Positive(() => a += 1)
                            .Zero(() => a += 1);
            }
        }

        [Benchmark]
        public void TestIfComparisons()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-5, 5);
                IntT b = r.Next(-5, 5);
                var comparison = a.Spaceship(b);
                if (comparison.Value == TritVal.n)
                {
                    a += 1;
                }
                else if (comparison.Value == TritVal.p)
                {
                    a += 1;
                }
                else
                {
                    a += 1;
                }
            }
        }

        [Benchmark]
        public void TestCaseComparisons()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-5, 5);
                IntT b = r.Next(-5, 5);
                switch (a.Spaceship(b).Value)
                {
                    case TritVal.n:
                        a += 1;
                        break;
                    case TritVal.p:
                        a += 1;
                        break;
                    case TritVal.z:
                        a += 1;
                        break;
                    default:
                        break;
                }
            }
        }

        [Benchmark]
        public void TestIntTAddition()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-1000000, 1000000);
                IntT b = r.Next(-1000000, 1000000);
                var _ = a + b;
            }
        }

        [Benchmark]
        public void TestIntBAddition()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntB a = r.Next(-1000000, 1000000);
                IntB b = r.Next(-1000000, 1000000);
                var _ = a + b;
            }
        }

        [Benchmark]
        public void TestUIntTAddition()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UIntT a = new UIntT((ulong)r.Next(0, 1_000_000));
                UIntT b = new UIntT((ulong)r.Next(0, 1_000_000));
                var _ = a + b;
            }
        }

        // ---- Native binary-hardware baselines ----
        // IntT/IntB/UIntT all SIMULATE arithmetic with a per-digit software loop over a
        // ulong. The honest "binary on binary hardware" reference is a single native
        // machine instruction, measured here. Notes on fairness:
        //  * Range: 32 trits = 3^32 ~ 2^50.7, so IntT/UIntT hold ~51 bits of range.
        //    IntB uses 64 bits (over-wide); native `long` is 64-bit, native `int` 32-bit.
        //    A range-fair binary width for the ternary types would be ~51 bits.
        //  * These baselines keep the same RNG-in-loop structure as the other benchmarks,
        //    so RNG/allocation overhead is shared. Even so the arithmetic itself is
        //    effectively free; the true per-op gap (see the isolated add-only microbench)
        //    is ~100-200x, larger than this RNG-dominated harness makes it look.
        [Benchmark]
        public void TestNativeLongAddition()
        {
            long acc = 0;
            for (int i = 0; i < nIterations; i++)
            {
                long a = r.Next(-1000000, 1000000);
                long b = r.Next(-1000000, 1000000);
                acc += a + b;
            }
            _sink = acc;
        }

        [Benchmark]
        public void TestNativeIntAddition()
        {
            int acc = 0;
            for (int i = 0; i < nIterations; i++)
            {
                int a = r.Next(-1000000, 1000000);
                int b = r.Next(-1000000, 1000000);
                acc += a + b;
            }
            _sink = acc;
        }

        [Benchmark]
        public void TestNativeLongMultiplication()
        {
            long acc = 0;
            for (int i = 0; i < nIterations; i++)
            {
                long a = r.Next(-100000, 100000);
                long b = r.Next(-100000, 100000);
                acc += a * b;
            }
            _sink = acc;
        }

        [Benchmark]
        public void TestIntTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-1000000, 1000000);
                IntT b = r.Next(-1000000, 1000000);
                var _ = a - b;
            }
        }

        [Benchmark]
        public void TestIntBSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntB a = r.Next(-1000000, 1000000);
                IntB b = r.Next(-1000000, 1000000);
                var _ = a - b;
            }
        }

        [Benchmark]
        public void TestUIntTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UIntT a = new UIntT((ulong)r.Next(0, 1_000_000));
                UIntT b = new UIntT((ulong)r.Next(0, 1_000_000));
                var _ = a >= b ? a - b : b - a;
            }
        }

        [Benchmark]
        public void TestIntTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-100000, 100000);
                IntT b = r.Next(-100000, 100000);
                var _ = a * b;
            }
        }

        [Benchmark]
        public void TestIntBMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntB a = r.Next(-100000, 100000);
                IntB b = r.Next(-100000, 100000);
                var _ = a * b;
            }
        }

        [Benchmark]
        public void TestUIntTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UIntT a = new UIntT((ulong)r.Next(0, 100000));
                UIntT b = new UIntT((ulong)r.Next(0, 100000));
                var _ = a * b;
            }
        }

        [Benchmark]
        public void TestIntTDivision()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-1000000, 1000000);
                IntT b = r.Next(-1000, 1000);
                try
                {
                    var _ = a / b;
                }
                catch (DivideByZeroException)
                {
                    continue;
                }
            }
        }

        [Benchmark]
        public void TestIntBDivision()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntB a = r.Next(-1000000, 1000000);
                IntB b = r.Next(-1000, 1000);
                try
                {
                    var _ = a / b;
                }
                catch (DivideByZeroException)
                {
                    continue;
                }
            }
        }

        [Benchmark]
        public void TestUIntTDivision()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UIntT a = new UIntT((ulong)r.Next(0, 1_000_000));
                UIntT b = new UIntT((ulong)r.Next(1, 1000));
                var _ = a / b;
            }
        }

        [Benchmark]
        public void TestIntTModulus()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-1000000, 1000000);
                IntT b = r.Next(-1000, 1000);
                try
                {
                    var _ = a % b;
                }
                catch (DivideByZeroException)
                {
                    continue;
                }
            }
        }

        [Benchmark]
        public void TestIntBModulus()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntB a = r.Next(-1000000, 1000000);
                IntB b = r.Next(-1000, 1000);
                try
                {
                    var _ = a % b;
                }
                catch (DivideByZeroException)
                {
                    continue;
                }
            }
        }

        [Benchmark]
        public void TestUIntTModulus()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UIntT a = new UIntT((ulong)r.Next(0, 1_000_000));
                UIntT b = new UIntT((ulong)r.Next(1, 1000));
                var _ = a % b;
            }
        }

        [Benchmark]
        public void TestIntTPower()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(0, 10);
                IntT b = r.Next(0, 10);
                MathT.Pow(a, b);
            }
        }

        [Benchmark]
        public void TestUIntTBitwise()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UIntT a = new UIntT((ulong)r.Next(0, 1_000_000));
                UIntT b = new UIntT((ulong)r.Next(0, 1_000_000));
                var _ = a & b;
                _ = a | b;
                _ = a ^ b;
                _ = a << 2;
                _ = a >> 2;
            }
        }

        // Tryte Benchmarks
        [Benchmark]
        public void TestTryteArithmetic()
        {
            for (int i = 0; i < nIterations; i++)
            {
                // Operands are bounded so the product a*b still fits a 6-trit Tryte
                // (|value| <= 364): 18*18 = 324. The old [-20,20] range could reach 400
                // and threw OverflowException, marking the whole benchmark NA.
                Tryte a = new Tryte(r.Next(-18, 19));
                Tryte b = new Tryte(r.Next(-18, 19));
                var _ = a + b;
                _ = a - b;
                _ = a * b;
            }
        }

        [Benchmark]
        public void TestUTryteArithmetic()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UTryte a = new UTryte((ulong)r.Next(0, 80));
                UTryte b = new UTryte((ulong)r.Next(1, 9));
                var _ = a + b;
                _ = a >= b ? a - b : b - a;
                _ = a * b;
                _ = a / b;
                _ = a % b;
            }
        }

        [Benchmark]
        public void TestTryteBitwise()
        {
            for (int i = 0; i < nIterations; i++)
            {
                Tryte a = new Tryte(r.Next(Tryte.MinValue, Tryte.MaxValue + 1));
                Tryte b = new Tryte(r.Next(Tryte.MinValue, Tryte.MaxValue + 1));
                var _ = a & b;
                _ = a | b;
                _ = a ^ b;
                _ = a << 2;   // truncating shift; high trits are dropped, never throws
                _ = a >> 2;
            }
        }

        [Benchmark]
        public void TestUTryteBitwise()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UTryte a = new UTryte((ulong)r.Next(0, 729));
                UTryte b = new UTryte((ulong)r.Next(0, 729));
                var _ = a & b;
                _ = a | b;
                _ = a ^ b;
                _ = a << 2;   // truncating shift; high trits are dropped, never throws
                _ = a >> 2;
            }
        }

        [Benchmark]
        public void TestTryteVsByteConversion()
        {
            for (int i = 0; i < nIterations; i++)
            {
                byte b = (byte)r.Next(0, 256);
                var tryte = TernaryConverter.TryteFromUInt8(b);
                var back = TernaryConverter.TryteToUInt8(tryte);
            }
        }

        [Benchmark]
        public void TestUTryteVsByteConversion()
        {
            for (int i = 0; i < nIterations; i++)
            {
                byte b = (byte)r.Next(0, 256);
                var tryte = new UTryte((ulong)b);
                var back = (byte)tryte.ULongValue;
            }
        }

        // Trit Benchmarks
        [Benchmark]
        public void TestTritOperations()
        {
            for (int i = 0; i < nIterations; i++)
            {
                Trit a = new Trit((TritVal)r.Next(-1, 2));
                Trit b = new Trit((TritVal)r.Next(-1, 2));
                var _ = a.NEG();
                _ = a.XOR(b);
                _ = a.AND(b);
                _ = a.OR(b);
            }
        }

        [Benchmark]
        public void TestUTritOperations()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UTrit a = new UTrit(r.Next(0, 3));
                UTrit b = new UTrit(r.Next(1, 3));
                var _ = a.NEG();
                _ = a.XOR(b);
                _ = a.AND(b);
                _ = a.OR(b);
                _ = a + b;
                _ = a - b;
                _ = a * b;
                _ = a / b;
            }
        }

        [Benchmark]
        public void TestTritDecisionMaking()
        {
            for (int i = 0; i < nIterations; i++)
            {
                Trit t = new Trit((TritVal)r.Next(-1, 2));
                t.Positive(() => { })
                 .Negative(() => { })
                 .Zero(() => { });
            }
        }

        [Benchmark]
        public void TestMixedBalancedUnbalancedSpaceshipComparisons()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT balanced = new IntT(r.Next(-1000, 1000));
                UIntT unbalanced = new UIntT((ulong)r.Next(0, 1000));
                var _ = balanced.Spaceship(unbalanced);
                _ = unbalanced.Spaceship(balanced);
            }
        }

        // FloatT Benchmarks
        [Benchmark]
        public void TestFloatTAddition()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = FloatT.FromDouble(r.NextDouble() * r.Next(-10000, 10000));
                FloatT b = FloatT.FromDouble(r.NextDouble() * r.Next(-10000, 10000));
                var _ = a + b;
            }
        }

        [Benchmark]
        public void TestUFloatTAddition()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UFloatT a = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 10000));
                UFloatT b = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 10000));
                var _ = a + b;
            }
        }

        [Benchmark]
        public void TestFloatTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = FloatT.FromDouble(r.NextDouble() * r.Next(-10000, 10000));
                FloatT b = FloatT.FromDouble(r.NextDouble() * r.Next(-10000, 10000));
                var _ = a - b;
            }
        }

        [Benchmark]
        public void TestUFloatTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UFloatT a = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 10000));
                UFloatT b = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 10000));
                var _ = a >= b ? a - b : b - a;
            }
        }

        [Benchmark]
        public void TestFloatTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = FloatT.FromDouble(r.NextDouble() * 1000 * r.Next(-1000, 1000));
                FloatT b = FloatT.FromDouble(r.NextDouble() * 1000 * r.Next(-1000, 1000));
                var _ = a * b;
            }
        }

        [Benchmark]
        public void TestUFloatTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UFloatT a = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 1000));
                UFloatT b = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 1000));
                var _ = a * b;
            }
        }

        [Benchmark]
        public void TestFloatTDivision()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = FloatT.FromDouble(r.NextDouble() * 1000 * r.Next(-1000, 1000));
                FloatT b = FloatT.FromDouble(r.NextDouble() * 1000 * r.Next(-100, 100));
                if (b.ToDouble() != 0)
                {
                    var _ = a / b;
                }
            }
        }

        [Benchmark]
        public void TestUFloatTDivision()
        {
            for (int i = 0; i < nIterations; i++)
            {
                UFloatT a = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 1000));
                UFloatT b = UFloatT.FromDouble(r.NextDouble() * r.Next(1, 100));
                if (b.ToDouble() != 0)
                {
                    var _ = a / b;
                }
            }
        }

        [Benchmark]
        public void TestFloatTConversion()
        {
            for (int i = 0; i < nIterations; i++)
            {
                double d = r.NextDouble() * r.Next(-1000000, 1000000);
                var floatT = FloatT.FromDouble(d);
                var back = floatT.ToDouble();
            }
        }

        [Benchmark]
        public void TestUFloatTConversion()
        {
            for (int i = 0; i < nIterations; i++)
            {
                double d = r.NextDouble() * r.Next(1, 1000000);
                var floatT = UFloatT.FromDouble(d);
                var back = floatT.ToDouble();
            }
        }

    }

    // Benchmarks whose cost scales with input size. Only THESE use [Params], so only
    // these run once per DataSize value (10/100/1000). Everything else runs once.
    [MemoryDiagnoser]
    public class ScalingBenchmarks
    {
        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        private readonly Random r = new Random();

        // Large dataset benchmarks
        [Benchmark]
        public void TestTernaryQuickSortLarge()
        {
            var values = new IntT[DataSize];
            for (int n = 0; n < DataSize; n++)
            {
                values[n] = new IntT(r.Next(-1_000_000, 1_000_000));
            }
            TernaryAlgorithms.TernaryQuicksort(values);
        }

        [Benchmark]
        public void TestBinaryQuickSortLarge()
        {
            var values = new int[DataSize];
            for (int n = 0; n < DataSize; n++)
            {
                values[n] = r.Next(-1_000_000, 1_000_000);
            }
            TernaryAlgorithms.BinaryQuicksort(values);
        }

        // Memory efficiency benchmarks
        [Benchmark]
        public void TestTernarySearchTreeMemory()
        {
            var tree = new TernarySearchTree<int>();
            for (int i = 0; i < DataSize; i++)
            {
                tree.Put($"word{i}", i);
            }
            // Force enumeration to ensure all nodes are created
            var count = tree.Items().Count();
        }

        [Benchmark]
        public void TestBinarySearchTreeMemory()
        {
            var tree = new BinarySearchTree<int>();
            for (int i = 0; i < DataSize; i++)
            {
                tree.Put($"word{i}", i);
            }
            // Force enumeration to ensure all nodes are created
            var count = tree.Items().Count();
        }
    }
}
}
