using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class QrDouble
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Reconstruction(int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, n * n).Select(i => random.NextDouble()).ToArray();
            var tau = new double[n];

            var ma = CreateMatrix(a, n);

            Matrix<double> mr;

            fixed (double* pa = a)
            fixed (double* ptau = tau)
            {
                Lapack.Dgeqrf(
                    MatrixLayout.ColMajor,
                    n, n,
                    pa, n,
                    ptau);

                mr = CreateMatrix(a, n);

                Lapack.Dorgqr(
                    MatrixLayout.ColMajor,
                    n, n, n,
                    pa, n,
                    ptau);
            }

            for (var col = 0; col < n; col++)
            {
                for (var row = col + 1; row < n; row++)
                {
                    mr[row, col] = 0;
                }
            }

            var mq = CreateMatrix(a, n);
            var reconstructed = mq * mr;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    Assert.That(reconstructed[row, col], Is.EqualTo(ma[row, col]).Within(1.0E-12));
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
