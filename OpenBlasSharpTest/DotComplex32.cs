using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;
using OpenBlasSharp;

using MComplex32 = MathNet.Numerics.Complex32;

namespace OpenBlasSharpTest
{
    public class DotComplex32
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Normal(int length)
        {
            var random = new Random(42);

            var x = Enumerable.Range(0, length).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var y = Enumerable.Range(0, length).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();

            var vx = DenseVector.OfArray(x);
            var vy = DenseVector.OfArray(y);
            var expected = vx.DotProduct(vy);

            Complex32 actual;
            fixed (MComplex32* px = x)
            fixed (MComplex32* py = y)
            {
                actual = Blas.Cdotu(length, px, 1, py, 1);
            }

            Assert.That(actual.Real, Is.EqualTo(expected.Real).Within(1.0E-6));
            Assert.That(actual.Imaginary, Is.EqualTo(expected.Imaginary).Within(1.0E-6));
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Conjugated(int length)
        {
            var random = new Random(42);

            var x = Enumerable.Range(0, length).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var y = Enumerable.Range(0, length).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();

            var vx = DenseVector.OfArray(x);
            var vy = DenseVector.OfArray(y);
            var expected = vx.ConjugateDotProduct(vy);

            Complex32 actual;
            fixed (MComplex32* px = x)
            fixed (MComplex32* py = y)
            {
                actual = Blas.Cdotc(length, px, 1, py, 1);
            }

            Assert.That(actual.Real, Is.EqualTo(expected.Real).Within(1.0E-6));
            Assert.That(actual.Imaginary, Is.EqualTo(expected.Imaginary).Within(1.0E-6));
        }
    }
}
