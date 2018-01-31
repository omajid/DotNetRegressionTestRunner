using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    class ArgumentsAndOptions
    {
        public String DotNetExecutible { get; set; }
        public String TestRoot { get; set; }

        public bool Verbose { get; set; } = false;
    }

    class ProcessExecutionResult {
        public int ExitCode { get; set; }
        public string Command { get; set; }
        public StreamReader StandardOutput { get; set; }
        public StreamReader StandardError { get; set; }
    }

    class CompileResult
    {
        public bool Success { get; set; }
        public DirectoryInfo WorkingDirectory { get; set; }
        public String Configuration { get; set; } // Debug, Release

        // TODO public String TargetFramework { get; set; }

        public string Output { get; set; }
    }

    class ExecutionResult
    {
        public FileInfo File { get; }
        public bool Success { get; }
        public CompileResult CompileResult { get; }
        public String Output { get; }

        public ExecutionResult(FileInfo file, bool success, CompileResult compileResult, String output)
        {
            this.File = file;
            this.Success = success;
            this.CompileResult = compileResult;
            this.Output = output;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                PrintUsage(Console.Error);
                Environment.Exit(1);
            }

            var options = ParseArgumentsAndOptions(args);

            var dotnet = new FileInfo(options.DotNetExecutible);
            var testRoot = new DirectoryInfo(options.TestRoot);

            var workingDirectory = new DirectoryInfo(
                Path.Combine(Directory.GetCurrentDirectory(), "dotnetreg." + DateTimeOffset.Now.ToUnixTimeMilliseconds()));

            Console.WriteLine("Testing: " + dotnet);
            Console.WriteLine("Running tests at: " + testRoot);
            Console.WriteLine("Working directory: " + workingDirectory);

            var tests = FindTests(testRoot);
            var results = ExecuteTests(dotnet, workingDirectory, tests);

            PrintResults(results, Console.Out, Console.Error);

            Environment.Exit(0);
        }

        public static void PrintUsage(TextWriter output)
        {
            output.WriteLine("Usage: dotnet DotNetRegressionTestRunner /path/to/dotnet /path/to/tests");
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

            result.DotNetExecutible = toProcess[0];
            result.TestRoot = Path.GetFullPath(toProcess[1]);

            return result;
        }

        public static List<FileInfo> FindTests(DirectoryInfo testRoot)
        {
            return FindCSharpFiles(testRoot)
                .Where(Test.FileIsATest)
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

        public static List<ExecutionResult> ExecuteTests(FileInfo dotnetRoot, DirectoryInfo workingDirectory, List<FileInfo> testFilePaths)
        {
            var results = new List<ExecutionResult>();
            var originalCurrentDirectory = Directory.GetCurrentDirectory();

            foreach (var test in testFilePaths)
            {
                var newDirectory = new DirectoryInfo(Path.Combine(workingDirectory.FullName, Path.GetRandomFileName()));
                newDirectory.Create();

                var compileResult = CompileTest(dotnetRoot, newDirectory, test);
                if (compileResult.Success)
                {
                    results.Add(ExecuteTest(test, dotnetRoot, compileResult));
                }
                else
                {
                    var failedToCompile = new ExecutionResult(test, false, compileResult, null);
                    results.Add(failedToCompile);
                }
            }
            Directory.SetCurrentDirectory(originalCurrentDirectory);

            return results;
        }

        public static CompileResult CompileTest(FileInfo dotnetRoot, DirectoryInfo workingDirectory, FileInfo testFile)
        {
            var output = "";

            Directory.SetCurrentDirectory(workingDirectory.FullName);

            // TODO select the runtime to target

            var result = Exec($"{dotnetRoot}/dotnet", "new console");
            output += CreateCommandOutput(result);

            if (result.ExitCode != 0)
            {
                return new CompileResult
                {
                    Success = false,
                    WorkingDirectory = workingDirectory,
                    Output = output,
                };
            }

            new FileInfo(Path.Combine(workingDirectory.FullName, "Program.cs")).Delete();

            testFile.CopyTo(Path.Combine(workingDirectory.FullName, testFile.Name));

            result = Exec($"{dotnetRoot}/dotnet", "build");
            output += CreateCommandOutput(result);

            if (result.ExitCode != 0)
            {
                return new CompileResult
                {
                    Success = false,
                    WorkingDirectory = workingDirectory,
                    Output = output,
                };
            }

            return new CompileResult
            {
                Success = true,
                WorkingDirectory = workingDirectory,
                Output = output,
            };
        }

        public static ExecutionResult ExecuteTest(FileInfo test, FileInfo dotnetRoot, CompileResult compileResult) 
        {
            Directory.SetCurrentDirectory(compileResult.WorkingDirectory.FullName);
            var applicationName = compileResult.WorkingDirectory.Name;

            var result = Exec($"{dotnetRoot}/dotnet", $"bin/Debug/netcoreapp2.0/{applicationName}.dll");
            var output = CreateCommandOutput(result);

            return new ExecutionResult(test, (result.ExitCode == 0), compileResult, output);

        }

        public static void PrintResults(List<ExecutionResult> results, TextWriter output, TextWriter error)
        {
            // foreach (var result in results)
            // {
            //     output.WriteLine("Compiling: \n" + result.CompileResult.Output);
            //     output.WriteLine("Executing: \n" + result.Output);
            // }
            foreach (var result in results)
            {
                if (result.Success)
                {
                    output.WriteLine("Pass:   " + result.File);
                }
                else
                {
                    output.WriteLine("FAILED: " + result.File);
                }
            }
        }

        public static ProcessExecutionResult Exec(string command, string arguments)
        {
            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();

                return new ProcessExecutionResult
                {
                    Command = $"{command} {arguments}",
                    ExitCode = process.ExitCode,
                    StandardError = process.StandardError,
                    StandardOutput = process.StandardOutput,
                };
            }
        }

        public static string CreateCommandOutput(ProcessExecutionResult result)
        {
            var output = "";
            output += result.Command + "\n";
            output += "Exit code: " + result.ExitCode;
            output += "\n=== stdout ===\n";
            output += result.StandardOutput.ReadToEnd();
            output += "\n";
            output += "\n=== stderr ===\n";
            output += result.StandardError.ReadToEnd();
            output += "\n";
            return output;
        }
    }
}
