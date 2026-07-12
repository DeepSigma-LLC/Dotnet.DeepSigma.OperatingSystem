using System.Diagnostics;
using DeepSigma.Core.Monads;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// Provides methods to run terminal commands and capture their output.
/// </summary>
public static class Terminal
{
    /// <summary>
    /// Contains constants for common terminal commands.
    /// Note: These constants are provided for convenience and may not be available on all systems. Use with caution and ensure the commands are installed on the target system.
    /// These commands do not include any file extensions (e.g., .exe) and are intended to be used in a cross-platform manner. 
    /// This means that the commands should work on both Windows and Unix-like systems (Linux, macOS) without requiring any modifications.
    /// However, they may need to be added to the system's PATH environment variable to be recognized by the terminal. 
    /// If a command is not found, it may indicate that it is not installed or not in the PATH.
    /// </summary>
    public static class CommonExecutables
    {
        /// <summary>
        /// Pyhon executable.
        /// </summary>
        public const string Python = "python";

        /// <summary>
        /// Dotnet executable.
        /// </summary>
        public const string Dotnet = "dotnet";

        /// <summary>
        /// Git executable.
        /// </summary>
        public const string Git = "git";

        /// <summary>
        /// Node.js executable.
        /// </summary>
        public const string Node = "node";

        /// <summary>
        /// Npm executable.
        /// </summary>
        public const string Npm = "npm";

        /// <summary>
        /// Yarn executable.
        /// </summary>
        public const string Yarn = "yarn";

        /// <summary>
        /// Pip executable.
        /// </summary>
        public const string Pip = "pip";

        /// <summary>
        /// Cmd executable.
        /// </summary>
        public const string Cmd = "cmd";

        /// <summary>
        /// PowerShell executable.
        /// </summary>
        public const string PowerShell = "powershell";
    }

    /// <summary>
    /// Checks if a program is installed on the system by using the terminal 'where' command.
    /// </summary>
    /// <param name="program"></param>
    /// <returns></returns>
    public static bool IsProgramInstalled(string program)
    {
        return RunCommand("where", program).Match(
            success => true,
            error => false
        );
    }

    /// <summary>
    /// Runs a terminal command with optional arguments and captures the output.
    /// </summary>
    /// <param name="executable">The executable to run.</param>
    /// <param name="args">Optional arguments to pass to the executable.</param>
    /// <returns></returns>
    public static ResultMonad<string> RunCommand(string executable, string? args = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
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

        if (process.ExitCode == 0)
        {
            return new Success<string>(output.Trim());

        }
        return new Error(new Exception($"Error executing terminal command: {errors}"));
    }

    /// <summary>
    /// Runs a terminal command asynchronously with optional arguments, working directory, and cancellation support.
    /// </summary>
    /// <param name="executable">The executable to run.</param>
    /// <param name="args">Optional arguments to pass to the executable.</param>
    /// <param name="workingDirectory">Optional working directory for the command.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns></returns>
    public static async Task<ResultMonad<string>> RunCommandAsync(string executable, string? args = null, string? workingDirectory = null, CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = args ?? string.Empty,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrEmpty(workingDirectory))
            psi.WorkingDirectory = workingDirectory;

        using var process = new Process { StartInfo = psi };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(ct);
        var errorTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct).ConfigureAwait(false);

        string output = await outputTask.ConfigureAwait(false);
        string errors = await errorTask.ConfigureAwait(false);

        if (process.ExitCode == 0)
        {
            return new Success<string>(output.Trim());
        }
        return new Error(new Exception($"Error executing terminal command: {errors}"));
    }
}
