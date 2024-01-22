using System;
using System.Linq;
using System.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class MulComplex
    {
        [TestCase(1, 1, 1)]
        [TestCase(2, 2, 2)]
        [TestCase(3, 3, 3)]
        [TestCase(2, 3, 4)]
        [TestCase(3, 2, 4)]
        [TestCase(5, 3, 4)]
        [TestCase(3, 5, 4)]
        public unsafe void Test(int m, int n, int k)
        {
            var random = new Random(42);

            var a = Enumerable.Range(0, m * k).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();
            var b = Enumerable.Range(0, k * n).Select(i => new Complex(random.NextDouble(), random.NextDouble())).ToArray();
            var c = new Complex[m * n];

            var alpha = Complex.One;
            var beta = Complex.One;

            var ma = CreateMatrix(a, m, k);
            var mb = CreateMatrix(b, k, n);
            var expected = ma * mb;

            fixed (Complex* pa = a)
            fixed (Complex* pb = b)
            fixed (Complex* pc = c)
            {
                Blas.Zgemm(
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
                Assert.That(d.Real < 1.0E-12);
                Assert.That(d.Imaginary < 1.0E-12);
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
