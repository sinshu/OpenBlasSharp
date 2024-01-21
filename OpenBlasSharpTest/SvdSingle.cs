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
        [TestCase(2, 3)]
        [TestCase(4, 3)]
        public unsafe void Reconstruction(int m, int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * n).Select(i => random.NextSingle()).ToArray();
            var s = new float[Math.Min(m, n)];
            var u = new float[m * m];
            var vt = new float[n * n];
            var work = new float[Math.Min(m, n) - 1];

            var am = CreateMatrix(a, m, n);

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
                    pa, Math.Max(1, m),
                    ps,
                    pu, m,
                    pvt, n,
                    pwork);
            }

            var sm = CreateDiagonalMatrix(s, m, n);
            var um = CreateMatrix(u, m, m);
            var vtm = CreateMatrix(vt, n, n);
            var reconstructed = um * sm * vtm;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < m; row++)
                {
                    var error = am[row, col] - reconstructed[row, col];
                    Assert.That(Math.Abs(error) < 1.0E-6);
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
