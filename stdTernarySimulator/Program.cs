using System;
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

            // Uncomment to run the benchmarks
            // var summary = BenchmarkRunner.Run<TernaryBenchmarks>();
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
    }

    [MemoryDiagnoser]
    public class TernaryBenchmarks
    {
        private readonly int nIterations = 500;
        private readonly Random r = new Random();

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
        public void TestFloatTAddition()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = r.NextSingle() * r.Next(-10000, 10000);
                FloatT b = r.NextSingle() * r.Next(-10000, 10000);
                var _ = a + b;
            }
        }

        [Benchmark]
        public void TestFloatTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = r.NextSingle() * r.Next(-10000, 10000);
                FloatT b = r.NextSingle() * r.Next(-10000, 10000);
                var _ = a - b;
            }
        }

        [Benchmark]
        public void TestFloatTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
                FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
                var _ = a * b;
            }
        }

        [Benchmark]
        public void TestFloatTDivision()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
                FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-100, 100));
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
        public void TestFloatTModulus()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
                FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-100, 100));
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
    }
}
