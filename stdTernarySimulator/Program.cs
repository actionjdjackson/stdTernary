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
            ReportComparison("IntT", a, b);
            ReportComparison("IntT", b, a);
            ReportComparison("IntT", a, new IntT(120));

            FloatT c = 3.14159;
            FloatT d = 2.71828;
            ReportComparison("FloatT", c, d);
            ReportComparison("FloatT", d, c);
            ReportComparison("FloatT", c, (FloatT)3.14159);

            var summary = BenchmarkRunner.Run<TernaryBenchmarks>();
        }

        private static void ReportComparison<T>(string label, T left, T right) where T : IComparable<T>
        {
            int comparison = left.CompareTo(right);
            string relation = comparison switch
            {
                > 0 => ">",
                < 0 => "<",
                _ => "="
            };

            Console.WriteLine($"{label}: {left} {relation} {right}");
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
