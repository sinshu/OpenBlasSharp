using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class QrDouble
    {
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(4, 4)]
        [TestCase(5, 5)]
        [TestCase(3, 2)]
        [TestCase(5, 3)]
        public unsafe void Reconstruction(int m, int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * n).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();
            var tau = new Complex[n];

            var ma = CreateMatrix(a, m, n);

            Complex[] result;
            fixed (Complex* pa = a)
            fixed (Complex* ptau = tau)
            {
                Lapack.Zgeqrf(
                    MatrixLayout.ColMajor,
                    m, n,
                    pa, m,
                    ptau);

                result = a.ToArray();

                Lapack.Zungqr(
                    MatrixLayout.ColMajor,
                    m, n, n,
                    pa, m,
                    ptau);
            }

            Matrix<Complex> mr = new DenseMatrix(n, n);
            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row <= col; row++)
                {
                    var i = m * col + row;
                    mr[row, col] = result[i];
                }
            }

            var mq = CreateMatrix(a, m, n);
            var reconstructed = mq * mr;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    Assert.That(reconstructed[row, col].Real, Is.EqualTo(ma[row, col].Real).Within(1.0E-12));
                    Assert.That(reconstructed[row, col].Imaginary, Is.EqualTo(ma[row, col].Imaginary).Within(1.0E-12));
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
    }
}
