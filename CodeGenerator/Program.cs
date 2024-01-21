using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CodeGenerator
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            LapackGenerator.Run();
        }
    }
}
