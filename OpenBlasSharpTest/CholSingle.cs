using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class CholSingle
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

            fixed (float* pa = a)
            {
                Lapack.Spotrf(
                    MatrixLayout.ColMajor,
                    'L',
                    n,
                    pa, n);
            }

            var ml = CreateMatrixFromL(a, n);
            var reconstructed = ml * ml.Transpose();

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    Assert.That(reconstructed[row, col], Is.EqualTo(ma[row, col]).Within(1.0E-6));
                }
            }
        }

        private static float[] CreateHermitianMatrixData(Random random, int n)
        {
            var data = new float[n * n];
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
                        data[i1] = random.NextSingle();
                        data[i2] = data[i1];
                    }
                }
            }
            return data;
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

        private static Matrix<float> CreateMatrixFromL(float[] values, int n)
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
