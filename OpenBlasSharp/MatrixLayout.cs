using System;

namespace OpenBlasSharp
{
    /// <summary>
    /// Specifies the matrix layout.
    /// </summary>
    public enum MatrixLayout : int
    {
        /// <summary>
        /// Matrices are interpreted as row-major.
        /// </summary>
        RowMajor = 101,

        /// <summary>
        /// Matrices are interpreted as column-major.
        /// </summary>
        ColMajor = 102,
    }
}
