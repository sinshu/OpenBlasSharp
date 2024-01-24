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
    public class InvComplex32
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public unsafe void Test(int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, n * n).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var piv = new int[n];

            var ma = CreateMatrix(a, n);

            fixed (MComplex32* pa = a)
            fixed (int* ppiv = piv)
            {
                Lapack.Cgetrf(
                    MatrixLayout.ColMajor,
                    n, n,
                    (Complex32*)pa, n,
                    ppiv);

                Lapack.Cgetri(
                    MatrixLayout.ColMajor,
                    n,
                    (Complex32*)pa, n,
                    ppiv);
            }

            var mb = CreateMatrix(a, n);
            var mi = ma * mb;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < n; row++)
                {
                    var expected = row == col ? MComplex32.One : MComplex32.Zero;
                    Assert.That(mi[row, col].Real, Is.EqualTo(expected.Real).Within(1.0E-6));
                    Assert.That(mi[row, col].Imaginary, Is.EqualTo(expected.Imaginary).Within(1.0E-6));
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
