using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class SvdSingle
    {
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(4, 4)]
        [TestCase(5, 5)]
        [TestCase(3, 2)]
        [TestCase(5, 3)]
        [TestCase(4, 1)]
        [TestCase(2, 3)]
        [TestCase(3, 5)]
        [TestCase(1, 4)]
        public unsafe void Reconstruction(int m, int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * n).Select(i => random.NextSingle()).ToArray();
            var s = new float[Math.Min(m, n)];
            var u = new float[m * m];
            var vt = new float[n * n];
            var work = new float[Math.Min(m, n) - 1];

            var ma = CreateMatrix(a, m, n);

            fixed (float* pa = a)
            fixed (float* ps = s)
            fixed (float* pu = u)
            fixed (float* pvt = vt)
            fixed (float* pwork = work)
            {
                Lapack.Sgesvd(
                    MatrixLayout.ColMajor,
                    'A', 'A',
                    m, n,
                    pa, m,
                    ps,
                    pu, m,
                    pvt, n,
                    pwork);
            }

            var ms = CreateDiagonalMatrix(s, m, n);
            var mu = CreateMatrix(u, m, m);
            var mvt = CreateMatrix(vt, n, n);
            var reconstructed = mu * ms * mvt;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < m; row++)
                {
                    Assert.That(reconstructed[row, col], Is.EqualTo(ma[row, col]).Within(1.0E-6));
                }
            }
        }

        private static Matrix<float> CreateMatrix(float[] values, int m, int n)
        {
            var mat = new DenseMatrix(m, n);
            var i = 0;
            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < m; row++)
                {
                    mat[row, col] = values[i++];
                }
            }
            return mat;
        }

        private static Matrix<float> CreateDiagonalMatrix(float[] values, int m, int n)
        {
            var mat = new DenseMatrix(m, n);
            for (var i = 0; i < Math.Min(m, n); i++)
            {
                mat[i, i] = values[i];
            }
            return mat;
        }
    }
}
