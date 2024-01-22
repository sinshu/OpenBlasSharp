using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class SvdComplex
    {
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(2, 3)]
        [TestCase(4, 3)]
        public unsafe void Reconstruction(int m, int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * n).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();
            var s = new double[Math.Min(m, n)];
            var u = new Complex[m * m];
            var vt = new Complex[n * n];
            var work = new double[Math.Min(m, n) - 1];

            var ma = CreateMatrix(a, m, n);

            fixed (Complex* pa = a)
            fixed (double* ps = s)
            fixed (Complex* pu = u)
            fixed (Complex* pvt = vt)
            fixed (double* pwork = work)
            {
                Lapack.Zgesvd(
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
                    var error = ma[row, col] - reconstructed[row, col];
                    Assert.That(Math.Abs(error.Real) < 1.0E-12);
                    Assert.That(Math.Abs(error.Imaginary) < 1.0E-12);
                }
            }
        }

        private static Matrix<Complex> CreateMatrix(Complex[] values, int m, int n)
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

        private static Matrix<Complex> CreateDiagonalMatrix(double[] values, int m, int n)
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
