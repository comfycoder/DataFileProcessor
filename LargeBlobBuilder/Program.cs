using System;
using System.IO;

namespace LargeBlobBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var sw = new StreamWriter(@"c:\temp\bigblob.txt"))
            {
                for (int i = 0; i < 40_000_000; i++)
                {
                    sw.WriteLine("Some line we are not interested in processing");
                }

                sw.WriteLine("Data: 42");
            }
        }
    }
}
