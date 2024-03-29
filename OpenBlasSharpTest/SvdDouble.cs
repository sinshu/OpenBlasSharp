using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class SvdDouble
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

            var a = Enumerable.Range(0, m * n).Select(i => random.NextDouble()).ToArray();
            var s = new double[Math.Min(m, n)];
            var u = new double[m * m];
            var vt = new double[n * n];
            var work = new double[Math.Min(m, n) - 1];

            var ma = CreateMatrix(a, m, n);

            fixed (double* pa = a)
            fixed (double* ps = s)
            fixed (double* pu = u)
            fixed (double* pvt = vt)
            fixed (double* pwork = work)
            {
                Lapack.Dgesvd(
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
                    Assert.That(reconstructed[row, col], Is.EqualTo(ma[row, col]).Within(1.0E-12));
                }
            }
        }

        private static Matrix<double> CreateMatrix(double[] values, int m, int n)
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

        private static Matrix<double> CreateDiagonalMatrix(double[] values, int m, int n)
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
