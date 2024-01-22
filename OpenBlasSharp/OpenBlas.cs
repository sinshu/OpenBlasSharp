using System;
using System.Runtime.InteropServices;

namespace OpenBlasSharp
{
    /// <summary>
    /// Provides OpenBLAS specific functions.
    /// </summary>
    public static partial class OpenBlas
    {
        /// <summary>
        /// The name of the OpenBLAS library.
        /// </summary>
        public const string LibraryName = "libopenblas";

        /// <summary>
        /// Set the number of threads on runtime.
        /// </summary>
        /// <param name="numThreads">The number of threads.</param>
        [DllImport(LibraryName, EntryPoint = "openblas_set_num_threads", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetNumThreads(int numThreads);

        /// <summary>
        /// Get the number of threads on runtime.
        /// </summary>
        /// <returns>The number of threads.</returns>
        [DllImport(LibraryName, EntryPoint = "openblas_get_num_threads", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumThreads();

        /// <summary>
        /// Get the number of physical processors (cores).
        /// </summary>
        /// <returns>The number of processors.</returns>
        [DllImport(LibraryName, EntryPoint = "openblas_get_num_procs", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumProcs();

        /// <summary>
        /// Get the build configure on runtime.
        /// </summary>
        /// <returns>The build configure.</returns>
        public static string GetConfig()
        {
            return Marshal.PtrToStringAnsi(GetConfigCore());
        }

        /// <summary>
        /// Get the CPU corename on runtime.
        /// </summary>
        /// <returns>The CPU corename.</returns>
        public static string GetCorename()
        {
            return Marshal.PtrToStringAnsi(GetCorenameCore());
        }

        [DllImport(LibraryName, EntryPoint = "openblas_get_config", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetConfigCore();

        [DllImport(LibraryName, EntryPoint = "openblas_get_corename", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetCorenameCore();
    }
}
