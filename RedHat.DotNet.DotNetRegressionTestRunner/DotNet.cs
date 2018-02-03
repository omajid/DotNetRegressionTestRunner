using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public static bool IsValidDotNetHome(DirectoryInfo dotNetHome)
        {
            if (!dotNetHome.Exists)
            {
                return false;
            }

            var dotNetExecutible = new FileInfo(Path.Combine(dotNetHome.FullName, "dotnet"));
            return dotNetHome
                .EnumerateFiles()
                .Where(file => file.FullName == dotNetExecutible.FullName)
                .Any();
        }

        public static IEnumerable<System.Version> GetAvailableRuntimeVersions(DirectoryInfo dotNetHome)
        {
            var netCoreAppDir = new DirectoryInfo(Path.Combine(dotNetHome.FullName, "shared", "Microsoft.NETCore.App"));

            var runtimes = netCoreAppDir
                .EnumerateDirectories()
                .Select(dir => new Version(dir.Name))
                // remove patch update version numbers because that
                // will just make it harder for users to expect the
                // right thing with ranges such as: [,2.0]
                .Select(version => new Version(version.Major, version.Minor));

            return runtimes;
        }

        public static IEnumerable<string> GetAvailableFrameworks(DirectoryInfo dotNetHome)
        {
            // FIXME
            return new string[] { "netcoreapp2.0" };
        }
    }
}
