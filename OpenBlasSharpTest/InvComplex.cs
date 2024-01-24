using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class InvComplex
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Test(int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, n * n).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();
            var piv = new int[n];

            var ma = CreateMatrix(a, n);

            fixed (Complex* pa = a)
            fixed (int* ppiv = piv)
            {
                Lapack.Zgetrf(
                    MatrixLayout.ColMajor,
                    n, n,
                    pa, n,
                    ppiv);

                Lapack.Zgetri(
                    MatrixLayout.ColMajor,
                    n,
                    pa, n,
                    ppiv);
            }

            var mb = CreateMatrix(a, n);
            var mi = ma * mb;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    var expected = row == col ? Complex.One : Complex.Zero;
                    Assert.That(mi[row, col].Real, Is.EqualTo(expected.Real).Within(1.0E-12));
                    Assert.That(mi[row, col].Imaginary, Is.EqualTo(expected.Imaginary).Within(1.0E-12));
                }
            }
        }

        private static Matrix<Complex> CreateMatrix(Complex[] values, int n)
        {
            var mat = new DenseMatrix(n, n);
            var i = 0;
            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    mat[row, col] = values[i++];
                }
            }
            return mat;
        }
    }
}
