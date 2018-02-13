using System;
using System.IO;
using System.Linq;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class ArgumentsAndOptions
    {
        public String DotNetHome { get; set; }
        public String TestRoot { get; set; }

        public bool Verbose { get; set; } = false;

        /// <summary>
        /// Returns null if it can't parse the arguments properly
        /// </summary>
        public static ArgumentsAndOptions Parse(string[] arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }
            if (arguments.Count() == 0)
            {
                return null;
            }

            var result = new ArgumentsAndOptions();

            var toProcess = arguments.ToList();
            if (toProcess.Contains("--verbose"))
            {
                result.Verbose = true;
                toProcess.Remove("--verbose");
            }

            if (toProcess.Count() < 1)
            {
                return null;
            }

            result.TestRoot = Path.GetFullPath(toProcess[0]);
            toProcess.RemoveAt(0);

            if (toProcess.Count > 0)
            {
                result.DotNetHome = toProcess[0];
            }
            else
            {
                result.DotNetHome = DotNet.SystemDotNetPath;
            }

            return result;
        }
    }
}
