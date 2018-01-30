using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class VersionRange
    {
        public Version MinVersion { get; set; }
        public bool MinVersionInclusive { get; set; }
        public Version MaxVersion { get; set; }
        public bool MaxVersionInclusive { get; set; }

        public override string ToString()
        {
            return (MinVersionInclusive ? "[" : "(") + MinVersion + "," + MaxVersion + (MaxVersionInclusive ? "]" : ")");
        }

        public static VersionRange CombineConstraints(IEnumerable<String> constraints)
        {
            var versionRangePattern = @"^(=|>|>=|<|<=)(\d+\.\d+)$";

            var versionRange = new VersionRange();
            versionRange.MinVersion = new Version(0, 0);
            versionRange.MinVersionInclusive = true;
            versionRange.MaxVersion = new Version(int.MaxValue, int.MaxValue);
            versionRange.MaxVersionInclusive = true;
            
            foreach (var constraint in constraints)
            {
                var match = Regex.Match(constraint, versionRangePattern);
                if (!match.Success)
                {
                    throw new Exception("invalid version constraint in attribute: " + constraint);
                }

                var oper = match.Groups[1].Value;
                var rawVersion = match.Groups[2].Value;
                // Console.WriteLine("operator: " + oper + " version: " + rawVersion);
                bool success = Version.TryParse(rawVersion, out Version newVersion);
                if (!success)
                {
                    throw new Exception("Unable to parse version " + rawVersion);
                }

                switch (oper)
                {
                    case "=":
                        versionRange.MinVersion = versionRange.MaxVersion = newVersion;
                        versionRange.MinVersionInclusive = versionRange.MaxVersionInclusive = true;
                        break;
                    case "<=":
                        // MaxVersion comes after newVersion; make newVersion the new upper bound
                        if (versionRange.MaxVersion > newVersion)
                        {
                            versionRange.MaxVersion = newVersion;
                            versionRange.MaxVersionInclusive = true;
                        }
                        break;

                    case "<":
                        // MaxVersion comes after newVersion; make newVersion the new upper bound
                        if (versionRange.MaxVersion.CompareTo(newVersion) >= 0)
                        {
                            versionRange.MaxVersion = newVersion;
                            versionRange.MaxVersionInclusive = false;
                        }
                        break;
                    case ">=":
                        // MaxVersion comes before newVersion; make newVersion the new lower bound
                        if (versionRange.MinVersion.CompareTo(newVersion) <= 0)
                        {
                            versionRange.MinVersion = newVersion;
                            versionRange.MinVersionInclusive = true;
                        }
                        break;
                    case ">":
                        // MaxVersion comes before newVersion; make newVersion the new lower bound
                        if (versionRange.MinVersion.CompareTo(newVersion) <= 0)
                        {
                            versionRange.MinVersion = newVersion;
                            versionRange.MinVersionInclusive = false;
                        }
                        break;
                    default:
                        throw new Exception("Unrecognized operator " + oper);
                }
            }

            return versionRange;
        }

        public static bool IsVersionInRange(Version version, VersionRange range)
        {
            if ((version < range.MinVersion) || (version > range.MaxVersion))
            {
                return false;
            }

            if (version == range.MaxVersion && range.MaxVersionInclusive)
            {
                return true;
            }

            if (version == range.MinVersion && range.MinVersionInclusive)
            {
                return true;
            }

            if (version < range.MaxVersion && version > range.MinVersion)
            {
                return true;
            }

            return false;
        }

        static private System.Version GetCurrentRuntimeVersion()
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
