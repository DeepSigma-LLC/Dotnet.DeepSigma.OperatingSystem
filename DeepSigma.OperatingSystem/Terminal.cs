using System.Diagnostics;
using DeepSigma.General;

namespace DeepSigma.OperatingSystem
{
    public static  class Terminal
    {
        public static ResultMonad<string> RunCommand(string command, string? args = null)
        {
            var psi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args ?? string.Empty,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string errors = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (string.IsNullOrWhiteSpace(errors) == false)
            {
                return new Error(new ExceptionLogItem(new Exception(errors), "Error executing terminal command.")
                );
            }

            return new Success<string>(output.Trim());
        }

    }
}
