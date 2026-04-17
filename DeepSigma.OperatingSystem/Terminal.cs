using System.Diagnostics;
using DeepSigma.Core.Monads;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// Provides methods to run terminal commands and capture their output.
/// </summary>
public static class Terminal
{

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
    /// <param name="command"></param>
    /// <param name="args"></param>
    /// <returns></returns>
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

        if (process.ExitCode == 0)
        {
            return new Success<string>(output.Trim());

        }
        return new Error(new Exception($"Error executing terminal command: {errors}"));
    }

    /// <summary>
    /// Runs a terminal command asynchronously with optional arguments, working directory, and cancellation support.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="args"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<ResultMonad<string>> RunCommandAsync(string command, string? args = null, string? workingDirectory = null, CancellationToken ct = default)
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

        if (!string.IsNullOrEmpty(workingDirectory))
            psi.WorkingDirectory = workingDirectory;

        using var process = new Process { StartInfo = psi };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(ct);
        var errorTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        string output = await outputTask;
        string errors = await errorTask;

        if (process.ExitCode == 0)
        {
            return new Success<string>(output.Trim());
        }
        return new Error(new Exception($"Error executing terminal command: {errors}"));
    }
}
