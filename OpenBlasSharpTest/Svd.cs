using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class Svd
    {
        [Test]
        public unsafe void Test()
        {
            var n = 3;
            var random = new Random(42);

            var aData = Enumerable.Range(0, n * n).Select(i => random.NextDouble()).ToArray();
            var sData = new double[n];
            var uData = new double[n * n];
            var vtData = new double[n * n];
            var work = new double[100];

            var aMat = CreateSquareMatrix(aData, n);

            fixed (double* a = aData)
            fixed (double* s = sData)
            fixed (double* u = uData)
            fixed (double* vt = vtData)
            fixed (double* superb = work)
            {
                Lapack.Dgesvd(
                    MatrixLayout.ColMajor,
                    'A', 'A',
                    n, n,
                    a, n,
                    s,
                    u, n,
                    vt, n,
                    superb);
            }

            var sMat = CreateDiagonalMatrix(sData, n);
            var uMat = CreateSquareMatrix(uData, n);
            var vtMat = CreateSquareMatrix(vtData, n);
            var reconstructed = uMat * sMat * vtMat;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    var error = aMat[row, col] - reconstructed[row, col];
                    Assert.That(Math.Abs(error) < 1.0E-12);
                }
            }
        }

        private static Matrix<double> CreateSquareMatrix(double[] values, int n)
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

        private static Matrix<double> CreateDiagonalMatrix(double[] values, int n)
        {
            var mat = new DenseMatrix(n, n);
            for (var i = 0; i < n; i++)
            {
                mat[i, i] = values[i];
            }
            return mat;
        }
    }
}
