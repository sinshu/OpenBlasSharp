using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class MulDouble
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

            var a = Enumerable.Range(0, m * k).Select(i => random.NextDouble()).ToArray();
            var b = Enumerable.Range(0, k * n).Select(i => random.NextDouble()).ToArray();
            var c = new double[m * n];

            var ma = CreateMatrix(a, m, k);
            var mb = CreateMatrix(b, k, n);
            var expected = ma * mb;

            fixed (double* pa = a)
            fixed (double* pb = b)
            fixed (double* pc = c)
            {
                Blas.Dgemm(
                    Order.ColMajor,
                    Transpose.NoTrans,
                    Transpose.NoTrans,
                    m, n, k,
                    1.0,
                    pa, m,
                    pb, k,
                    1.0,
                    pc, m);
            }
            var actual = CreateMatrix(c, m, n);

            var error = expected - actual;
            foreach (var d in error.Enumerate())
            {
                Assert.That(d < 1.0E-12);
            }
        }

        private static Matrix<double> CreateMatrix(double[] values, int m, int n)
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
