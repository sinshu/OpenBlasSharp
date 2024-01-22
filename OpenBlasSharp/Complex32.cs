using System;
using System.Numerics;

namespace OpenBlasSharp
{
    /// <summary>
    /// Represents a complex number.
    /// </summary>
    public struct Complex32
    {
        private float real;
        private float imaginary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Complex32"/> structure using the specified real and imaginary values.
        /// </summary>
        /// <param name="real">The real part of the complex number.</param>
        /// <param name="imaginary">The imaginary part of the complex number.</param>
        public Complex32(float real, float imaginary)
        {
            this.real = real;
            this.imaginary = imaginary;
        }

        /// <summary>
        /// Gets the real component of the current <see cref="Complex32"/> object.
        /// </summary>
        public float Real => real;

        /// <summary>
        /// Gets the imaginary component of the current <see cref="Complex32"/> object.
        /// </summary>
        public float Imaginary => imaginary;
    }
}
