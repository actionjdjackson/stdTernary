using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using stdTernary;

namespace stdTernary
{
    public class Program
    {
        private static void Main(string[] args)
        {
            IntT a = 120;
            IntT b = 60;

            switch(IntT.COMPARET(a, b).Value)
            {
                case IntT.Larger:
                    { Console.WriteLine("a was larger than b"); }
                break;
                case IntT.Smaller:
                    { Console.WriteLine("a was smaller than b"); }
                break;
                case IntT.Equal:
                    { Console.WriteLine("a was equal to b"); }
                break;
                default:
                    { Console.WriteLine("else branch was executed"); }
                break;
            }

            switch(IntT.COMPARET(b, a).Value)
            {
                case IntT.Larger:
                    { Console.WriteLine("b was larger than a"); }
                break;
                case IntT.Smaller:
                    { Console.WriteLine("b was smaller than a"); }
                break;
                case IntT.Equal:
                    { Console.WriteLine("b was equal to a"); }
                break;
                default:
                    { Console.WriteLine("else branch was executed"); }
                break;
            }

            IntT.COMPARET(a, b)
                        .Larger(() => {Console.WriteLine("a was larger than b");})
                        .Equal(() => {Console.WriteLine("a was equal to b");})
                        .Smaller(() => {Console.WriteLine("a was smaller than b");});
            IntT.COMPARET(b, a)
                        .Larger(() => {Console.WriteLine("b was larger than a");})
                        .Equal(() => {Console.WriteLine("b was equal to a");})
                        .Smaller(() => {Console.WriteLine("b was smaller than a");});
            b = 120;
            IntT.COMPARET(a, b)
                        .Larger(() => {Console.WriteLine("a was larger than b");})
                        .Equal(() => {Console.WriteLine("a was equal to b");})
                        .LargerOrEqual(() => {Console.WriteLine("a was larger than or equal to b");})
                        .Smaller(() => { Console.WriteLine("a was smaller than b");});

            FloatT c = 3.14159;
            FloatT d = 2.71828;
            FloatT.COMPARET(c, d)
                        .Larger(() => {Console.WriteLine("c was larger than d");})
                        .Equal(() => {Console.WriteLine("c was equal to d");})
                        .Smaller(() => {Console.WriteLine("c was smaller than d");});
            FloatT.COMPARET(d, c)
                        .Larger(() => {Console.WriteLine("d was larger than c");})
                        .Equal(() => {Console.WriteLine("d was equal to c");})
                        .Smaller(() => {Console.WriteLine("d was smaller than c");});
            d = 3.14159;
            FloatT.COMPARET(c, d)
                        .Larger(() => {Console.WriteLine("c was larger than d");})
                        .Equal(() => {Console.WriteLine("c was equal to d");})
                        .Smaller(() => { Console.WriteLine("c was smaller than d");});    
            FloatT.COMPARET(c, d)
                        .Smaller(() => {Console.WriteLine("c was smaller than d");})
                        .Else(() => {Console.WriteLine("c was not smaller than d, took the else branch");});

            var summary = BenchmarkRunner.Run<TernaryBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class TernaryBenchmarks
    {
        int nIterations = 500;
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
                FloatT a = r.NextSingle() * r.Next(-10000, 10000);
                FloatT b = r.NextSingle() * r.Next(-10000, 10000);
                var c = a + b;
            }
        }

        [Benchmark]
        public void TestFloatTSubtraction()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = r.NextSingle() * r.Next(-10000, 10000);
                FloatT b = r.NextSingle() * r.Next(-10000, 10000);
                var c = a - b;
            }
        }

        [Benchmark]
        public void TestFloatTMultiplication()
        {
            for (int i = 0; i < nIterations; i++)
            {
                FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
                FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
                var c = a * b;
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
                FloatT a = (double)(r.NextSingle() * 1000 * r.Next(-1000, 1000));
                FloatT b = (double)(r.NextSingle() * 1000 * r.Next(-100, 100));
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