using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class DotDouble
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Test(int length)
        {
            var random = new Random(42);

            var x = Enumerable.Range(0, length).Select(i => random.NextDouble()).ToArray();
            var y = Enumerable.Range(0, length).Select(i => random.NextDouble()).ToArray();

            var vx = DenseVector.OfArray(x);
            var vy = DenseVector.OfArray(y);
            var expected = vx * vy;

            double actual;
            fixed (double* px = x)
            fixed (double* py = y)
            {
                actual = Blas.Ddot(length, px, 1, py, 1);
            }

            Assert.That(actual, Is.EqualTo(expected).Within(1.0E-12));
        }
    }
}
