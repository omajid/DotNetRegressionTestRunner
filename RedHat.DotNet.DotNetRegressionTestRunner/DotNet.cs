using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class DotNet
    {
        private string _baseDir;
        private SemVersion[] _runtimeVersions;
        private SemVersion[] _sdkVersions;
        private string[] _supportedFrameworks;

        public DotNet(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (Directory.Exists(path))
            {
                path = Path.Combine(path, "dotnet");
            }

            // follow symbolic links
            path = RealPath(path);

            _baseDir = Path.GetDirectoryName(path);
        }

        public SemVersion[] SdkVersions
        {
            get
            {
                if (_sdkVersions == null)
                {
                    _sdkVersions = ParseVersionsFromChildDirectories(Path.Combine(_baseDir, "sdk"));
                }
                return _sdkVersions;
            }
        }

        public SemVersion[] RuntimeVersions
        {
            get
            {
                if (_runtimeVersions == null)
                {
                    _runtimeVersions = ParseVersionsFromChildDirectories(Path.Combine(_baseDir, "shared", "Microsoft.NETCore.App"));
                }
                return _runtimeVersions;
            }
        }

        public string[] Frameworks
        {
            get
            {
                if (_supportedFrameworks == null)
                {
                    var versions = new HashSet<string>();
                    foreach (var version in RuntimeVersions)
                    {
                        versions.Add($"netcoreapp{version.Major}.{version.Minor}");
                    }
                    _supportedFrameworks = versions.ToArray();
                }
                return _supportedFrameworks;
            }
        }

        public SemVersion LatestSdk => SdkVersions.LastOrDefault();

        public string BaseDir => _baseDir;

        public string DotnetPath => Path.Combine(BaseDir, "dotnet");

        public bool IsValid => RuntimeVersions.Length > 0;

        public static string SystemDotNetPath
        {
            get
            {
                var pathDirectories = Environment.GetEnvironmentVariable("PATH")?.Split(':');
                if (pathDirectories != null)
                {
                    foreach (var path in pathDirectories)
                    {
                        if (File.Exists(Path.Combine(path, "dotnet")))
                        {
                            return path;
                        }
                    }
                }
                return null;
            }
        }

        public override string ToString() => DotnetPath;

        public ProcessExecutionResult Exec(string arguments) =>
            Utilities.Exec(DotnetPath, arguments);

        private static SemVersion[] ParseVersionsFromChildDirectories(string dirname)
        {
            if (!Directory.Exists(dirname))
            {
                return Array.Empty<SemVersion>();
            }
            var childDirs = Directory.GetDirectories(dirname);
            return ParseAndSortVersions(childDirs);
        }

        private static SemVersion[] ParseAndSortVersions(string[] names)
        {
            var versions = new List<SemVersion>();
            foreach (var name in names)
            {
                if (SemVersion.TryParse(Path.GetFileName(name), out SemVersion semver))
                {
                    versions.Add(semver);
                }
            }
            versions.Sort();
            return versions.ToArray();
        }

        private static unsafe string RealPath(string path)
        {
            byte* resolvedPath = stackalloc byte[4096];
            IntPtr rv = realpath(path, new IntPtr(resolvedPath));
            if (rv != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(rv);
            }
            else
            {
                return path;
            }
        }

        [DllImport("libc")]
        private static extern IntPtr realpath(string path, IntPtr resolvedPath);
    }
}
