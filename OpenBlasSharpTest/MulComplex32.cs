using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex32;
using NUnit.Framework;
using OpenBlasSharp;

using MComplex32 = MathNet.Numerics.Complex32;

namespace OpenBlasSharpTest
{
    public class MulComplex32
    {
        [TestCase(1, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(3, 3, 3)]
        [TestCase(2, 3, 4)]
        [TestCase(1, 3, 5)]
        [TestCase(4, 3, 2)]
        [TestCase(5, 3, 1)]
        [TestCase(2, 5, 2)]
        [TestCase(1, 4, 1)]
        [TestCase(5, 2, 5)]
        [TestCase(4, 1, 4)]
        public unsafe void Test(int m, int n, int k)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * k).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var b = Enumerable.Range(0, k * n).Select(i => new MComplex32(random.NextSingle(), random.NextSingle())).ToArray();
            var c = new MComplex32[m * n];

            var alpha = MComplex32.One;
            var beta = MComplex32.One;

            var ma = CreateMatrix(a, m, k);
            var mb = CreateMatrix(b, k, n);
            var expected = ma * mb;

            fixed (MComplex32* pa = a)
            fixed (MComplex32* pb = b)
            fixed (MComplex32* pc = c)
            {
                Blas.Cgemm(
                    Order.ColMajor,
                    Transpose.NoTrans,
                    Transpose.NoTrans,
                    m, n, k,
                    &alpha,
                    pa, m,
                    pb, k,
                    &beta,
                    pc, m);
            }
            var actual = CreateMatrix(c, m, n);

            var error = expected - actual;
            foreach (var d in error.Enumerate())
            {
                Assert.That(d.Real < 1.0E-6);
                Assert.That(d.Imaginary < 1.0E-6);
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
