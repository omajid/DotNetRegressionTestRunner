using System;
using System.Collections.Generic;
using System.IO;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    public class TestRunner
    {
        public static List<TestExecutionResult> ExecuteTests(DotNet dotnet, DirectoryInfo workingDirectory, List<TestInfo> tests, Action<TestExecutionResult> AfterEachTest)
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
                    var testRan = ExecuteTest(test, dotnet, compileResult);
                    AfterEachTest(testRan);
                    results.Add(testRan);
                }
                else
                {
                    var failedToCompile = new TestExecutionResult(test, false, compileResult, null);
                    AfterEachTest(failedToCompile);
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
