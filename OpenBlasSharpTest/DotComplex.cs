using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class DotComplex
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Normal(int length)
        {
            var random = new Random(42);

            var x = Enumerable.Range(0, length).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();
            var y = Enumerable.Range(0, length).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();

            var vx = DenseVector.OfArray(x);
            var vy = DenseVector.OfArray(y);
            var expected = vx.DotProduct(vy);

            Complex actual;
            fixed (Complex* px = x)
            fixed (Complex* py = y)
            {
                actual = Blas.Zdotu(length, px, 1, py, 1);
            }

            Assert.That(actual.Real, Is.EqualTo(expected.Real).Within(1.0E-12));
            Assert.That(actual.Imaginary, Is.EqualTo(expected.Imaginary).Within(1.0E-12));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Conjugated(int length)
        {
            var random = new Random(42);

            var x = Enumerable.Range(0, length).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();
            var y = Enumerable.Range(0, length).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();

            var vx = DenseVector.OfArray(x);
            var vy = DenseVector.OfArray(y);
            var expected = vx.ConjugateDotProduct(vy);

            Complex actual;
            fixed (Complex* px = x)
            fixed (Complex* py = y)
            {
                actual = Blas.Zdotc(length, px, 1, py, 1);
            }

            Assert.That(actual.Real, Is.EqualTo(expected.Real).Within(1.0E-12));
            Assert.That(actual.Imaginary, Is.EqualTo(expected.Imaginary).Within(1.0E-12));
        }
    }
}
