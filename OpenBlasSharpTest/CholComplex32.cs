using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;
using OpenBlasSharp;

using MComplex32 = MathNet.Numerics.Complex32;

namespace OpenBlasSharpTest
{
    public class CholComplex32
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Reconstruction(int n)
        {
            var random = new Random(42);

            var a = CreateHermitianMatrixData(random, n);

            var ma = CreateMatrix(a, n);

            fixed (MComplex32* pa = a)
            {
                Lapack.Cpotrf(
                    MatrixLayout.ColMajor,
                    'L',
                    n,
                    (Complex32*)pa, n);
            }

            var ml = CreateMatrixFromL(a, n);
            var reconstructed = ml * ml.ConjugateTranspose();

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    Assert.That(reconstructed[row, col].Real, Is.EqualTo(ma[row, col].Real).Within(1.0E-6));
                    Assert.That(reconstructed[row, col].Imaginary, Is.EqualTo(ma[row, col].Imaginary).Within(1.0E-6));
                }
            }
        }

        private static MComplex32[] CreateHermitianMatrixData(Random random, int n)
        {
            var data = new MComplex32[n * n];
            for (var col = 0; col < n; col++)
            {
                for (var row = col; row < n; row++)
                {
                    var i1 = n * col + row;
                    var i2 = n * row + col;
                    if (i1 == i2)
                    {
                        data[i1] = 2 + 2 * random.NextSingle();
                    }
                    else
                    {
                        data[i1] = new MComplex32(random.NextSingle(), random.NextSingle());
                        data[i2] = data[i1].Conjugate();
                    }
                }
            }
            return data;
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

        private static Matrix<MComplex32> CreateMatrixFromL(MComplex32[] values, int n)
        {
            var mat = new DenseMatrix(n, n);
            for (var col = 0; col < n; col++)
            {
                for (var row = col; row < n; row++)
                {
                    var i = n * col + row;
                    mat[row, col] = values[i];
                }
            }
            return mat;
        }
    }
}
