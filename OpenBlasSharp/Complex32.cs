using System;
using System.Numerics;

namespace OpenBlasSharp
{
    /// <summary>
    /// Represents a complex number.
    /// </summary>
    public struct Complex32
    {
        /// <summary>
        /// The real part of the complex number.
        /// </summary>
        public float Real;

        /// <summary>
        /// The imaginary part of the complex number.
        /// </summary>
        public float Imaginary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Complex32"/> structure using the specified real and imaginary values.
        /// </summary>
        /// <param name="real">The real part of the complex number.</param>
        /// <param name="imaginary">The imaginary part of the complex number.</param>
        public Complex32(float real, float imaginary)
        {
            this.Real = real;
            this.Imaginary = imaginary;
        }
    }
}
