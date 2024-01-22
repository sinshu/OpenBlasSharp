# OpenBlasSharp

The purpose of this project is to provide a .NET wrapper for [OpenBLAS](https://github.com/OpenMathLib/OpenBLAS).



## Installation

[The NuGet package](https://www.nuget.org/packages/OpenBlasSharp) is available.
This package does not contain the native DLL.
You must [download the compiled binary](https://github.com/OpenMathLib/OpenBLAS/releases) and put `libopenblas.dll` at the directory of the executable file.

All the classes are in the `OpenBlasSharp` namespace.

```cs
using OpenBlasSharp;
```



## Examples

### Dot product

```cs
var m = 3;
var n = 4;

var random = new Random(42);

var x = Enumerable.Range(0, length).Select(i => random.NextDouble()).ToArray();
var y = Enumerable.Range(0, length).Select(i => random.NextDouble()).ToArray();

fixed (double* px = x)
fixed (double* py = y)
{
    var result = Blas.Ddot(length, px, 1, py, 1);
}
```

### Matrix multiplication

```cs
var m = 3;
var n = 5;
var k = 4;

var random = new Random(42);

var a = Enumerable.Range(0, m * k).Select(i => random.NextDouble()).ToArray();
var b = Enumerable.Range(0, k * n).Select(i => random.NextDouble()).ToArray();
var c = new double[m * n];

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
```

### Singular value decomposition

```cs
var m = 4;
var n = 3;

var random = new Random(42);

var a = Enumerable.Range(0, m * n).Select(i => random.NextDouble()).ToArray();
var s = new double[Math.Min(m, n)];
var u = new double[m * m];
var vt = new double[n * n];
var work = new double[Math.Min(m, n) - 1];

fixed (double* pa = a)
fixed (double* ps = s)
fixed (double* pu = u)
fixed (double* pvt = vt)
fixed (double* pwork = work)
{
    Lapack.Dgesvd(
        MatrixLayout.ColMajor,
        'A', 'A',
        m, n,
        pa, m,
        ps,
        pu, m,
        pvt, n,
        pwork);
}
```

### OpenBLAS specific functions

```cs
var numThreads = OpenBlas.GetNumThreads();

Console.WriteLine(numThreads);
```



## Development status

* Doc comments for the BLAS functions are missing.
* Low-level LAPACK functions with the `_work` suffix are not supported.
* LAPACK functions that require function pointers are not supported.



## License
OpenBlasSharp is available under [the BSD-3-Clause license](LICENSE.txt).
