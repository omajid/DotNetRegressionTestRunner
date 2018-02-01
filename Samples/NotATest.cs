using System;

namespace Samples
{
    public class NotATest
    {
        static void Main(string[] args)
        {
            throw new Exception("Should not get run by framework");
        }
    }
}
