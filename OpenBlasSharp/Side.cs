using System;

namespace OpenBlasSharp
{
    /// <summary>
    /// Specifies whether the hermitian matrix appears on the left or right.
    /// </summary>
    public enum Side : int
    {
        /// <summary>
        /// The hermitian matrix appears on the left.
        /// </summary>
        Left = 141,

        /// <summary>
        /// The hermitian matrix appears on the right.
        /// </summary>
        Right = 142,
    }
}
