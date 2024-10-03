using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using stdTernary;

namespace stdTernary
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<TernaryBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class TernaryBenchmarks
    {
        int nIterations = 1000;
        Random r = new Random();

        [Benchmark]
        public void TestIntTAddition()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-1000000, 1000000);
                IntT b = r.Next(-1000000, 1000000);
                var c = a + b;
            }
        }

        [Benchmark]
        public void TestIntTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-1000000, 1000000);
                IntT b = r.Next(-1000000, 1000000);
                var c = a - b;
            }
        }

        [Benchmark]
        public void TestIntTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                IntT a = r.Next(-100000, 100000);
                IntT b = r.Next(-100000, 100000);
                var c = a * b;
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
                    var c = a / b;                
                }
                catch (System.DivideByZeroException)
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
                    var c = a % b;
                }
                catch (System.DivideByZeroException)
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
                FloatT a = r.NextDouble() * r.Next(-10000, 10000);
                FloatT b = r.NextDouble() * r.Next(-10000, 10000);
                var c = a + b;
            }
        }

        [Benchmark]
        public void TestFloatTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = r.NextDouble() * r.Next(-10000, 10000);
                FloatT b = r.NextDouble() * r.Next(-10000, 10000);
                var c = a - b;
            }
        }

        [Benchmark]
        public void TestFloatTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = r.NextDouble() * r.Next(-10000, 10000);
                FloatT b = r.NextDouble() * r.Next(-10000, 10000);
                var c = a * b;
            }
        }

        [Benchmark]
        public void TestFloatTDivision()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = r.NextDouble() * r.Next(-10000, 10000);
                FloatT b = r.NextDouble() * r.Next(-100, 100);
                try
                {
                    var c = a / b;                
                }
                catch (System.DivideByZeroException)
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
                FloatT a = r.NextDouble() * r.Next(-10000, 10000);
                FloatT b = r.NextDouble() * r.Next(-100, 100);
                try
                {
                    var c = a % b;                
                }
                catch (System.DivideByZeroException)
                {
                    continue;
                }
            }
        }

    }
}