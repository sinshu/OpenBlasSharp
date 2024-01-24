using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class CholComplex
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

            fixed (Complex* pa = a)
            {
                Lapack.Zpotrf(
                    MatrixLayout.ColMajor,
                    'L',
                    n,
                    pa, n);
            }

            var ml = CreateMatrixFromL(a, n);
            var reconstructed = ml * ml.ConjugateTranspose();

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    Assert.That(reconstructed[row, col].Real, Is.EqualTo(ma[row, col].Real).Within(1.0E-12));
                    Assert.That(reconstructed[row, col].Imaginary, Is.EqualTo(ma[row, col].Imaginary).Within(1.0E-12));
                }
            }
        }

        private static Complex[] CreateHermitianMatrixData(Random random, int n)
        {
            var data = new Complex[n * n];
            for (var col = 0; col < n; col++)
            {
                for (var row = col; row < n; row++)
                {
                    var i1 = n * col + row;
                    var i2 = n * row + col;
                    if (i1 == i2)
                    {
                        data[i1] = 2 + 2 * random.NextDouble();
                    }
                    else
                    {
                        data[i1] = new Complex(random.NextDouble(), random.NextDouble());
                        data[i2] = data[i1].Conjugate();
                    }
                }
            }
            return data;
        }

        private static Matrix<Complex> CreateMatrix(Complex[] values, int n)
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

        private static Matrix<Complex> CreateMatrixFromL(Complex[] values, int n)
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
