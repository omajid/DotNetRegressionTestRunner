// <test>
// <compile configuration="release"/>
// </test>

using System;

namespace Samples
{
    public class TestThatPassesInReleaseMode
    {
        static void Main(string[] args)
        {
            #if (DEBUG)
            throw new Exception("Should not happen");
            #endif

            Console.WriteLine("Hello World!");
        }
    }
}
