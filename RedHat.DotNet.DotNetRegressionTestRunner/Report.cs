using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{
    class Report
    {
        public static string GenerateReport(DotNet dotnet, List<TestExecutionResult> results)
        {
            var report = new StringBuilder();

            // TODO what more information would be useful to have?

            report.AppendLine("Test Report");
            report.AppendLine("Generated on " + DateTime.Now);
            report.AppendLine();

            report.AppendLine("Tested dotnet at: " + dotnet.DotnetPath);
            var runtimes = dotnet.RuntimeVersions
                .Select(ver => ver.ToString())
                .Aggregate((s1, s2) => s1 + ", " + s2);
            report.AppendLine("Found Runtimes: " + runtimes);
            var sdks = dotnet.SdkVersions
                .Select(ver => ver.ToString())
                .Aggregate((s1, s2) => s1 + ", " + s2);
            report.AppendLine("Found SDKs: " + sdks);
            report.AppendLine();
            report.AppendLine("dotnet --info:");
            report.AppendLine(dotnet.Exec("--info").StandardOutput.ReadToEnd());
            report.AppendLine();
            report.AppendLine("dotnet --version:");
            report.AppendLine(dotnet.Exec("--version").StandardOutput.ReadToEnd());


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

            var total = results.Count();
            var passed = results.Where(result => result.Success).Count();
            var failed = results.Where(result => !result.Success).Count();

            report.AppendLine();
            report.AppendLine("Total: " + total + ", Passed: " + passed + ", Failed: " + failed);
            report.AppendLine();

            return report.ToString();
        }

    }
}