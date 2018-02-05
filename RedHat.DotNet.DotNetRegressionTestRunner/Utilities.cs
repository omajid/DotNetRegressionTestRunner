using System.Diagnostics;
using System.IO;

namespace RedHat.DotNet.DotNetRegressionTestRunner
{

    public class ProcessExecutionResult {
        public int ExitCode { get; set; }
        public string Command { get; set; }
        public StreamReader StandardOutput { get; set; }
        public StreamReader StandardError { get; set; }
    }

    class Utilities
    {
        
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
    }
}
