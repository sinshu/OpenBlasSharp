using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;
using OpenBlasSharp;

using MComplex32 = MathNet.Numerics.Complex32;

namespace OpenBlasSharpTest
{
    public class SvdComplex32
    {
        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(2, 3)]
        [TestCase(4, 3)]
        public unsafe void Reconstruction(int m, int n)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * n).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var s = new float[Math.Min(m, n)];
            var u = new MComplex32[m * m];
            var vt = new MComplex32[n * n];
            var work = new float[Math.Min(m, n) - 1];

            var am = CreateMatrix(a, m, n);

            fixed (MComplex32* pa = a)
            fixed (float* ps = s)
            fixed (MComplex32* pu = u)
            fixed (MComplex32* pvt = vt)
            fixed (float* pwork = work)
            {
                Lapack.Cgesvd(
                    MatrixLayout.ColMajor,
                    'A', 'A',
                    m, n,
                    (Complex32*)pa, m,
                    ps,
                    (Complex32*)pu, m,
                    (Complex32*)pvt, n,
                    pwork);
            }

            var sm = CreateDiagonalMatrix(s, m, n);
            var um = CreateMatrix(u, m, m);
            var vtm = CreateMatrix(vt, n, n);
            var reconstructed = um * sm * vtm;

            for (var col = 0; col < n; col++)
            {
                for (var row = 0; row < m; row++)
                {
                    var error = am[row, col] - reconstructed[row, col];
                    Assert.That(Math.Abs(error.Real) < 1.0E-6);
                    Assert.That(Math.Abs(error.Imaginary) < 1.0E-6);
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

        private static Matrix<MComplex32> CreateDiagonalMatrix(float[] values, int m, int n)
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
