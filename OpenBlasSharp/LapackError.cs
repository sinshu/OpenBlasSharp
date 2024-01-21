using System;

namespace OpenBlasSharp
{
    public enum LapackInfo : int
    {
        None = 0,
        WorkMemoryError = -1010,
        TransposeMemoryError = -1011,
    }
}
