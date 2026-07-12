using DeepSigma.Core.Monads;
using System.Diagnostics;
using System.Text;
using System;

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
    /// Checks if a program is installed by asking the OS to resolve it on PATH.
    /// Uses 'where' on Windows and 'which' elsewhere.
    /// </summary>
    public static bool IsProgramInstalled(string program)
    {
        var locator = System.OperatingSystem.IsWindows() ? "where" : "which";
        return RunCommand(locator, program).Match(
            success => true,
            error => false
        );
    }

    /// <summary>
    /// Runs a terminal command synchronously. Delegates to the async implementation
    /// so both stdout and stderr are drained concurrently (avoids the pipe deadlock).
    /// </summary>
    public static ResultMonad<string> RunCommand(
        string executable,
        string? args = null,
        string? workingDirectory = null)
        => RunCommandAsync(executable, args, workingDirectory).GetAwaiter().GetResult();

    /// <summary>
    /// Runs a terminal command asynchronously with optional arguments, working directory,
    /// and cancellation support. Kills the process tree if cancelled.
    /// </summary>
    public static async Task<ResultMonad<string>> RunCommandAsync(
        string executable,
        string? args = null,
        string? workingDirectory = null,
        CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = args ?? string.Empty,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,   // so the child can't block waiting on console input
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        if (!string.IsNullOrEmpty(workingDirectory))
            psi.WorkingDirectory = workingDirectory;

        try
        {
            using var process = new Process { StartInfo = psi };

            if (!process.Start())
                return new Error(new Exception($"Failed to start process '{executable}'."));

            process.StandardInput.Close();

            // Start BOTH reads before waiting. Reading them sequentially deadlocks
            // as soon as the child fills the other pipe's buffer (~4 KB).
            var outputTask = process.StandardOutput.ReadToEndAsync(ct);
            var errorTask = process.StandardError.ReadToEndAsync(ct);

            try
            {
                await process.WaitForExitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
                throw;
            }

            var output = (await outputTask.ConfigureAwait(false)).Trim();
            var errors = (await errorTask.ConfigureAwait(false)).Trim();

            if (process.ExitCode == 0)
            {
                // Many tools (git, npm, pip) write informational text to stderr on success.
                // Fall back to it rather than returning an empty string.
                return new Success<string>(output.Length > 0 ? output : errors);
            }

            var detail = errors.Length > 0 ? errors : output;
            return new Error(new Exception(
                $"'{executable}' exited with code {process.ExitCode}: {detail}"));
        }
        catch (Exception ex)
        {
            // Win32Exception when the executable isn't on PATH, IOException, etc.
            return new Error(ex);
        }
    }
}