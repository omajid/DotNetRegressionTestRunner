using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class VersionRange
    {
        public Version MinVersion { get; set; } = new Version(0, 0);
        public bool MinVersionInclusive { get; set; } = true;
        public Version MaxVersion { get; set; } = new Version(int.MaxValue, int.MaxValue);
        public bool MaxVersionInclusive { get; set; } = true;

        public override string ToString()
        {
            return (MinVersionInclusive ? "[" : "(") + MinVersion + "," + MaxVersion + (MaxVersionInclusive ? "]" : ")");
        }

        public bool IsInRange(Version version)
        {
            if ((version < MinVersion) || (version > MaxVersion))
            {
                return false;
            }

            if (version == MaxVersion && MaxVersionInclusive)
            {
                return true;
            }

            if (version == MinVersion && MinVersionInclusive)
            {
                return true;
            }

            if (version < MaxVersion && version > MinVersion)
            {
                return true;
            }

            return false;
        }

        public static VersionRange Parse(string version)
        {
            var versionRangePattern = @"^(\[|\()(\d+\.\d+)?,(\d+\.\d+)?(\]|\))$";

            var versionRange = new VersionRange();

            var match = Regex.Match(version, versionRangePattern);
            if (!match.Success)
            {
                throw new Exception("invalid version constraint in attribute: " + version);
            }

            if (match.Groups[1].Value == "(")
            {
                versionRange.MinVersionInclusive = false;
            }
            var minVersion = match.Groups[2].Value;
            if (!string.IsNullOrEmpty(minVersion))
            {
                bool success = Version.TryParse(minVersion, out Version newVersion);
                if (!success)
                {
                    throw new Exception("Unable to parse version " + minVersion);
                }

                versionRange.MinVersion = newVersion;
            }

            var maxVersion = match.Groups[3].Value;
            if (!string.IsNullOrEmpty(maxVersion))
            {
                bool success = Version.TryParse(maxVersion, out Version newVersion);
                if (!success)
                {
                    throw new Exception("Unable to parse version " + maxVersion);
                }

                versionRange.MaxVersion = newVersion;
            }

            if (match.Groups[4].Value == ")")
            {
                versionRange.MaxVersionInclusive = false;
            }

            return versionRange;
        }
    }
}
