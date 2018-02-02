using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class DotNet
    {
        public static string FindBestSDKVersion()
        {
            // TODO FIXME
            return "2.0.3";
        }

        public static System.Version GetCurrentRuntimeVersion()
        {
            var assembly = typeof(System.Runtime.GCSettings).Assembly;
            // codebase looks like
            // file:///usr/lib64/dotnet/shared/Microsoft.NETCore.App/2.0.3/System.Private.CoreLib.dll
            var codebase = assembly.CodeBase;
            var pathParts = codebase.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var index = Array.IndexOf(pathParts, "Microsoft.NETCore.App");
            if (index != -1)
            {
                var fullCurrentVersion = new Version(pathParts[index + 1]);
                var currentVersion = new Version(fullCurrentVersion.Major, fullCurrentVersion.Minor);
                return currentVersion;
            }
            return null;
        }

    }
}
