using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

namespace stdTernary
{
    public class Program
    {
        private static void Main(string[] args)
        {
            IntT a = new IntT(120);
            IntT b = new IntT(60);

            DemoSpaceship("IntT", a, b);
            DemoMethodChaining("IntT", a, b);

            FloatT c = 3.14159;
            FloatT d = 2.71828;

            DemoSpaceship("FloatT", c, d);
            DemoMethodChaining("FloatT", c, d);

            //DemoBinaryPrimitives();

            //DemoSorting();
            //DemoTernarySearchTree();

            // Uncomment to run the benchmarks
            var summary = BenchmarkRunner.Run<TernaryBenchmarks>();
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
    }

    [MemoryDiagnoser]
    public class TernaryBenchmarks
    {
        private readonly int nIterations = 100;
        private readonly Random r = new Random();
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
        public void TestIntTPower()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(0, 10);
                IntT b = r.Next(0, 10);
                MathT.Pow(a, b);
            }
        }

    //     [Benchmark]
    //     public void TestFloatTAddition()
    //     {
    //         for (int i = 0; i < nIterations; i++)
    //         {
    //             FloatT a = r.NextSingle() * r.Next(-10000, 10000);
    //             FloatT b = r.NextSingle() * r.Next(-10000, 10000);
    //             var _ = a + b;
    //         }
    //     }

    //     [Benchmark]
    //     public void TestFloatTSubtraction()
    //     {
    //         for (int i = 0; i < nIterations; i++)
    //         {
    //             FloatT a = r.NextSingle() * r.Next(-10000, 10000);
    //             FloatT b = r.NextSingle() * r.Next(-10000, 10000);
    //             var _ = a - b;
    //         }
    //     }

    //     [Benchmark]
    //     public void TestFloatTMultiplication()
    //     {
    //         for (int i = 0; i < nIterations; i++)
    //         {
    //             FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
    //             FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
    //             var _ = a * b;
    //         }
    //     }

    //     [Benchmark]
    //     public void TestFloatTDivision()
    //     {
    //         for (int i = 0; i < nIterations; i++)
    //         {
    //             FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
    //             FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-100, 100));
    //             try
    //             {
    //                 var _ = a / b;
    //             }
    //             catch (DivideByZeroException)
    //             {
    //                 continue;
    //             }
    //         }
    //     }

    //     [Benchmark]
    //     public void TestFloatTModulus()
    //     {
    //         for (int i = 0; i < nIterations; i++)
    //         {
    //             FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
    //             FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-100, 100));
    //             try
    //             {
    //                 var _ = a % b;
    //             }
    //             catch (DivideByZeroException)
    //             {
    //                 continue;
    //             }
    //         }
    //     }
    }
}
