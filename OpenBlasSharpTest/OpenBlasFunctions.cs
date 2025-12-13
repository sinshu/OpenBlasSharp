using System;
using NUnit.Framework;
using OpenBlasSharp;

namespace OpenBlasSharpTest
{
    public class OpenBlasFunctions
    {
        [Test]
        public unsafe void ShowPlatform()
        {
            switch (sizeof(IntPtr))
            {
                case 4:
                    Console.WriteLine("x86");
                    break;
                case 8:
                    Console.WriteLine("x64");
                    break;
                default:
                    Console.WriteLine("???");
                    break;
            }
        }

        [Test]
        public void SetNumThreads()
        {
            OpenBlas.SetNumThreads(1);
        }

        [Test]
        public void GetNumThreads()
        {
            var value = OpenBlas.GetNumThreads();
            Console.WriteLine(value);
        }

        [Test]
        public void GetNumProcs()
        {
            var value = OpenBlas.GetNumProcs();
            Console.WriteLine(value);
        }

        [Test]
        public void GetConfig()
        {
            var value = OpenBlas.GetConfig();
            Console.WriteLine(value);
        }

        [Test]
        public void GetCorename()
        {
            var value = OpenBlas.GetCorename();
            Console.WriteLine(value);
        }
    }
}
