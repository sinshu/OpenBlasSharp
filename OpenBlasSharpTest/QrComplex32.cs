using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;
using OpenBlasSharp;

using MComplex32 = MathNet.Numerics.Complex32;

namespace OpenBlasSharpTest
{
    public class QrComplex32
    {
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(4, 4)]
        [TestCase(5, 5)]
        [TestCase(3, 2)]
        [TestCase(5, 3)]
        [TestCase(4, 1)]
        public unsafe void Reconstruction(int m, int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * n).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var tau = new MComplex32[n];

            var ma = CreateMatrix(a, m, n);

            MComplex32[] result;
            fixed (MComplex32* pa = a)
            fixed (MComplex32* ptau = tau)
            {
                Lapack.Cgeqrf(
                    MatrixLayout.ColMajor,
                    m, n,
                    (Complex32*)pa, m,
                    (Complex32*)ptau);

                result = a.ToArray();

                Lapack.Cungqr(
                    MatrixLayout.ColMajor,
                    m, n, n,
                    (Complex32*)pa, m,
                    (Complex32*)ptau);
            }

            Matrix<MComplex32> mr = new DenseMatrix(n, n);
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
                    Assert.That(reconstructed[row, col].Real, Is.EqualTo(ma[row, col].Real).Within(1.0E-6));
                    Assert.That(reconstructed[row, col].Imaginary, Is.EqualTo(ma[row, col].Imaginary).Within(1.0E-6));
                }
            }
        }

        private static Matrix<MComplex32> CreateMatrix(MComplex32[] values, int m, int n)
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
