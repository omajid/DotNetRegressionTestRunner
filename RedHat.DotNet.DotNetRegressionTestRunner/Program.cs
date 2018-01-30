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

    class CompileResult
    {
        public bool Success { get; set; }
        public string CompiledTarget { get; set; }
    }

    class ExecutionResult
    {
        public FileInfo File { get; }
        public bool Success { get; }
        public CompileResult CompileResult { get; }
        public String StandardOutput { get; }
        public String StandardError { get; }
        public Exception Exception { get; }

        public ExecutionResult(FileInfo file, bool success, CompileResult compileResult, String output, String error, Exception exception)
        {
            if (success && exception != null)
            {
                throw new ArgumentException("Exception on success", nameof(exception));
            }
            this.File = file;
            this.Success = success;
            this.CompileResult = compileResult;
            this.StandardOutput = output;
            this.StandardError = error;
            this.Exception = exception;
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

            var workingDirectory = new DirectoryInfo(Path.GetTempPath());

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

        public static List<ExecutionResult> ExecuteTests(FileInfo dotnet, DirectoryInfo workingDirectory, List<FileInfo> testFilePaths)
        {
            var results = new List<ExecutionResult>();
            foreach (var test in testFilePaths)
            {
                var compileResult = CompileTest(dotnet, workingDirectory, test);
                if (compileResult.Success)
                {
                    results.Add(ExecuteTest(dotnet, compileResult));
                }
                else
                {
                    var failedToCompile = new ExecutionResult(test, false, compileResult, null, null, null);
                    results.Add(failedToCompile);
                }
                    
            }

            return results;
        }

        public static CompileResult CompileTest(FileInfo dotnetRoot, DirectoryInfo workingDirectory, FileInfo testFile)
        {
            // TODO switch to some working directory layout
            var sdkVersion = DotNet.FindBestSDKVersion();
            var command = $"{dotnetRoot}/sdk/{sdkVersion}/Roslyn/RunCsc.sh";
            var arguments = $" -target:library {testFile.FullName}";
            Console.WriteLine("Invoking compiler: " + command);

            var compilerInfo = new ProcessStartInfo
            {
                WorkingDirectory = workingDirectory.FullName,
                UseShellExecute = false,
                FileName = command,
                Arguments = arguments,
            };

            using (var compiler = Process.Start(compilerInfo))
            {
                compiler.WaitForExit();
                var exitCode = compiler.ExitCode;
                if (exitCode != 0) {
                    return new CompileResult
                    {
                        CompiledTarget = "",
                        Success = true,
                    };
                }
                else
                {
                    
                    return new CompileResult
                    {
                        Success = false,
                    };
                }
            }
        }

        public static ExecutionResult ExecuteTest(FileInfo dotnet, CompileResult compileResult) 
        {
            var compiledFile = compileResult.CompiledTarget;
            return null;
        }

        public static void PrintResults(List<ExecutionResult> results, TextWriter output, TextWriter error)
        {
            
        }
    }
}
