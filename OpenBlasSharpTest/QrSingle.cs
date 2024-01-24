using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class QrSingle
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Reconstruction(int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, n * n).Select(i => random.NextSingle()).ToArray();
            var tau = new float[n];

            var ma = CreateMatrix(a, n);

            Matrix<float> mr;

            fixed (float* pa = a)
            fixed (float* ptau = tau)
            {
                Lapack.Sgeqrf(
                    MatrixLayout.ColMajor,
                    n, n,
                    pa, n,
                    ptau);

                mr = CreateMatrix(a, n);

                Lapack.Sorgqr(
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
                    Assert.That(reconstructed[row, col], Is.EqualTo(ma[row, col]).Within(1.0E-6));
                }
            }
        }

        private static Matrix<float> CreateMatrix(float[] values, int n)
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
