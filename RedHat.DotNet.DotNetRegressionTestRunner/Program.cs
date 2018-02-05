using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    class ArgumentsAndOptions
    {
        public String DotNetHome { get; set; }
        public String TestRoot { get; set; }

        public bool Verbose { get; set; } = false;
    }

    class TestInfo
    {
        public FileInfo File;
        public TestHeader Header;
    }

    class TestCompileResult
    {
        public bool Success { get; set; }
        public DirectoryInfo WorkingDirectory { get; set; }

        // TODO select sdk?

        public string Output { get; set; }
    }

    class TestExecutionResult
    {
        public TestInfo Test { get; }
        public bool Success { get; }
        public TestCompileResult CompileResult { get; }
        public String Output { get; }

        public TestExecutionResult(TestInfo test, bool success, TestCompileResult compileResult, String output)
        {
            this.Test = test;
            this.Success = success;
            this.CompileResult = compileResult;
            this.Output = output;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage(Console.Error);
                Environment.Exit(1);
            }

            var options = ParseArgumentsAndOptions(args);

            var dotnet = new DotNet(options.DotNetHome);
            var testRoot = new DirectoryInfo(options.TestRoot);

            if (!dotnet.IsValid)
            {
                Console.Error.WriteLine(dotnet + " does not look like a .NET Core home directory");
                PrintUsage(Console.Error);
                Environment.Exit(2);
            }

            var workingDirectory = new DirectoryInfo(
                Path.Combine(Directory.GetCurrentDirectory(), "dntr." + DateTimeOffset.Now.ToUnixTimeMilliseconds()));
            if (!workingDirectory.Exists)
            {
                workingDirectory.Create();
            }
            var reportFile = new FileInfo(Path.Combine(workingDirectory.FullName, "report.txt"));

            Console.WriteLine("Testing: " + dotnet);
            Console.WriteLine("Running tests at: " + testRoot);
            Console.WriteLine("Working directory: " + workingDirectory);
            Console.WriteLine("Full report at: " + reportFile);
            Console.WriteLine();

            var tests = FindTests(dotnet, testRoot, Console.Out);
            var results = ExecuteTests(dotnet, workingDirectory, tests);

            PrintSummary(results, Console.Out, Console.Error);
            WriteReport(results, reportFile);

            Environment.Exit(0);
        }

        public static void PrintUsage(TextWriter output)
        {
            output.WriteLine("Usage: dntr /path/to/tests [/path/to/dotnet]");
        }

        public static ArgumentsAndOptions ParseArgumentsAndOptions(string[] arguments)
        {
            var result = new ArgumentsAndOptions();

            var toProcess = arguments.ToList();
            if (toProcess.Contains("--verbose"))
            {
                result.Verbose = true;
                toProcess.Remove("--verbose");
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

        public static List<TestInfo> FindTests(DotNet dotnet, DirectoryInfo testRoot, TextWriter output)
        {
            return FindCSharpFiles(testRoot)
                .Select(file => new TestInfo { File = file, Header = TestParser.ParseTestHeader(file, output) })
                .Where(test => test.Header != null)
                .Where(test => test.Header.TargetsAvailableRuntime(dotnet))
                .Where(test => test.Header.TargetsAvailableFramework(dotnet))
                .ToList();
        }

        public static IEnumerable<FileInfo> FindCSharpFiles(DirectoryInfo root)
        {
            if (!root.Exists)
            {
                throw new ArgumentException("bad file search root " + root);
            }
            var files = root.EnumerateFiles("*.cs", SearchOption.AllDirectories);
            return files;
        }

        public static List<TestExecutionResult> ExecuteTests(DotNet dotnet, DirectoryInfo workingDirectory, List<TestInfo> tests)
        {
            var results = new List<TestExecutionResult>();
            var originalCurrentDirectory = Directory.GetCurrentDirectory();

            foreach (var test in tests)
            {
                var newDirectory = new DirectoryInfo(Path.Combine(workingDirectory.FullName, Path.GetRandomFileName()));
                newDirectory.Create();

                var compileResult = CompileTest(dotnet, newDirectory, test);
                if (compileResult.Success)
                {
                    results.Add(ExecuteTest(test, dotnet, compileResult));
                }
                else
                {
                    var failedToCompile = new TestExecutionResult(test, false, compileResult, null);
                    results.Add(failedToCompile);
                }
            }
            Directory.SetCurrentDirectory(originalCurrentDirectory);

            return results;
        }

        public static TestCompileResult CompileTest(DotNet dotnet, DirectoryInfo workingDirectory, TestInfo test)
        {
            var output = "";
            TestHeader header = test.Header;
            var configurationCommand = " -c " + header.Configuration;
            var frameworkCommand = " -f " + header.TargetFramework;

            Directory.SetCurrentDirectory(workingDirectory.FullName);

            // TODO select the runtime to target

            var result = dotnet.Exec("new console");
            output += CreateCommandOutput(result);

            if (result.ExitCode != 0)
            {
                return new TestCompileResult
                {
                    Output = output,
                    Success = false,
                    WorkingDirectory = workingDirectory,
                };
            }

            new FileInfo(Path.Combine(workingDirectory.FullName, "Program.cs")).Delete();

            test.File.CopyTo(Path.Combine(workingDirectory.FullName, test.File.Name));

            result = dotnet.Exec("build" + configurationCommand + frameworkCommand);
            output += CreateCommandOutput(result);

            return new TestCompileResult
            {
                Output = output,
                Success = (result.ExitCode == 0),
                WorkingDirectory = workingDirectory,
            };
        }

        public static TestExecutionResult ExecuteTest(TestInfo test, DotNet dotnet, TestCompileResult compileResult)
        {
            Directory.SetCurrentDirectory(compileResult.WorkingDirectory.FullName);
            var applicationName = compileResult.WorkingDirectory.Name;
            var configuration = test.Header.Configuration;
            var targetFramework = test.Header.TargetFramework;

            var result = dotnet.Exec($"bin/{configuration}/{targetFramework}/{applicationName}.dll");
            var output = CreateCommandOutput(result);

            return new TestExecutionResult(test, (result.ExitCode == 0), compileResult, output);
        }

        public static void PrintSummary(List<TestExecutionResult> results, TextWriter output, TextWriter error)
        {
            var total = 0;
            var passed = 0;
            var failed = 0;

            foreach (var result in results)
            {
                total++;
                if (result.Success)
                {
                    passed++;
                    output.WriteLine("Pass:   " + result.Test.File);
                }
                else
                {
                    failed++;
                    output.WriteLine("FAILED: " + result.Test.File);
                }
            }

            output.WriteLine();
            output.WriteLine("Total: " + total + " Passed: " + passed + " Failed: " + failed);
            output.WriteLine();
        }

        public static void WriteReport(List<TestExecutionResult> results, FileInfo report)
        {
            File.WriteAllText(report.FullName, GenerateReport(results));
        }

        public static string GenerateReport(List<TestExecutionResult> results)
        {
            var report = new StringBuilder();

            // TODO what more information would be useful to have?

            report.AppendLine("Test Report");
            report.AppendLine("Generated on " + DateTime.Now);
            report.AppendLine();

            foreach (var result in results)
            {
                report.AppendLine("# Test: " + result.Test.File);
                report.AppendLine("# Compiling: \n" + result.CompileResult.Output);
                if (result.CompileResult.Success)
                {
                    report.AppendLine("# Executing: \n" + result.Output);
                }
                report.AppendLine();
            }

            return report.ToString();
        }

        public static string CreateCommandOutput(ProcessExecutionResult result)
        {
            var output = "";
            output += result.Command + "\n";
            output += "Exit code: " + result.ExitCode + "\n";
            output += "=== stdout ===\n";
            output += result.StandardOutput.ReadToEnd() + "\n";
            output += "=== stdout end ===\n";
            output += "=== stderr ===\n";
            output += result.StandardError.ReadToEnd() + "\n";
            output += "=== stderr end ===\n";
            output += "\n";
            return output;
        }
    }
}
