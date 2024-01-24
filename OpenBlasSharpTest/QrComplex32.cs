using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;
using OpenBlasSharp;

using MComplex32 = MathNet.Numerics.Complex32;

namespace OpenBlasSharpTest
{
    public class QrComplex32
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Reconstruction(int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, n * n).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var tau = new MComplex32[n];

            var ma = CreateMatrix(a, n);

            Matrix<MComplex32> mr;

            fixed (MComplex32* pa = a)
            fixed (MComplex32* ptau = tau)
            {
                Lapack.Cgeqrf(
                    MatrixLayout.ColMajor,
                    n, n,
                    (Complex32*)pa, n,
                    (Complex32*)ptau);

                mr = CreateMatrix(a, n);

                Lapack.Cungqr(
                    MatrixLayout.ColMajor,
                    n, n, n,
                    (Complex32*)pa, n,
                    (Complex32*)ptau);
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
                    Assert.That(reconstructed[row, col].Real, Is.EqualTo(ma[row, col].Real).Within(1.0E-6));
                    Assert.That(reconstructed[row, col].Imaginary, Is.EqualTo(ma[row, col].Imaginary).Within(1.0E-6));
                }
            }
        }

        private static Matrix<MComplex32> CreateMatrix(MComplex32[] values, int n)
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
