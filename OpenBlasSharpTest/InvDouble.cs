using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class InvDouble
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Test(int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, n * n).Select(i => random.NextDouble()).ToArray();
            var piv = new int[n];

            var ma = CreateMatrix(a, n);

            fixed (double* pa = a)
            fixed (int* ppiv = piv)
            {
                Lapack.Dgetrf(
                    MatrixLayout.ColMajor,
                    n, n,
                    pa, n,
                    ppiv);

                Lapack.Dgetri(
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
                    var expected = row == col ? 1.0 : 0.0;
                    Assert.That(mi[row, col], Is.EqualTo(expected).Within(1.0E-12));
                }
            }
        }

        private static Matrix<double> CreateMatrix(double[] values, int n)
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
