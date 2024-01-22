using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class DotSingle
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Test(int length)
        {
            var random = new Random(42);

            var x = Enumerable.Range(0, length).Select(i => random.NextSingle()).ToArray();
            var y = Enumerable.Range(0, length).Select(i => random.NextSingle()).ToArray();

            var vx = DenseVector.OfArray(x);
            var vy = DenseVector.OfArray(y);
            var expected = vx * vy;

            float actual;
            fixed (float* px = x)
            fixed (float* py = y)
            {
                actual = Blas.Sdot(length, px, 1, py, 1);
            }

            Assert.That(actual, Is.EqualTo(expected).Within(1.0E-6));
        }
    }
}
