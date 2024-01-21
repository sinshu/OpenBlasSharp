using System;

namespace OpenBlasSharp
{
    /// <summary>
    /// Indicates the execution result of a LAPACK function.
    /// </summary>
    public enum LapackInfo : int
    {
        /// <summary>
        /// No error.
        /// </summary>
        None = 0,

        /// <summary>
        /// Failed to allocate working memory.
        /// </summary>
        WorkMemoryError = -1010,

        /// <summary>
        /// Failed to allocate memory to transpose matrices.
        /// </summary>
        TransposeMemoryError = -1011,
    }
}
