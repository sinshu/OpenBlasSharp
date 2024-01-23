using System;

namespace OpenBlasSharp
{
    /// <summary>
    /// Specifies whether or not the matrix is unit triangular.
    /// </summary>
    public enum Diag : int
    {
        /// <summary>
        /// The matrix is not assumed to be unit triangular.
        /// </summary>
        NonUnit = 131,

        /// <summary>
        /// The matrix is assumed to be unit triangular.
        /// </summary>
        Unit = 132,
    }
}
