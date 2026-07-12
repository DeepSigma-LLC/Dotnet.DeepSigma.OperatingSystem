using DeepSigma.Core.Monads;
using System.Diagnostics;
using System.Text;
using System;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// Provides methods to run commands in a terminal and capture their output or errors.
/// </summary>
public static class Terminal
{
    /// <summary>
    /// Run a command in a terminal and return the output or error.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <returns>The result of the command execution.</returns>
    public static ResultMonad<string> Run(TerminalCommand command)
        => RunAsync(command).GetAwaiter().GetResult();

    /// <summary>
    /// Run a command in a terminal and return the output or error.
    /// </summary>
    /// <param name="command">The command to run.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>The result of the command execution.</returns>
    public static async Task<ResultMonad<string>> RunAsync(
        TerminalCommand command,
        CancellationToken ct = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = command.FileName,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        foreach (var arg in command.Arguments)
            psi.ArgumentList.Add(arg); // no quoting to get wrong

        foreach (var (key, value) in command.Environment)
            psi.Environment[key] = value;

        if (!string.IsNullOrEmpty(command.WorkingDirectory))
            psi.WorkingDirectory = command.WorkingDirectory;

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(command.Timeout);

        try
        {
            using var process = new Process { StartInfo = psi };

            if (!process.Start())
                return new Error(new Exception($"Failed to start: {command}"));

            process.StandardInput.Close(); // nothing can block waiting on input

            var outputTask = process.StandardOutput.ReadToEndAsync(timeoutCts.Token);
            var errorTask = process.StandardError.ReadToEndAsync(timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return new Error(new TimeoutException($"Timed out after {command.Timeout}: {command}"));
            }

            var output = (await outputTask.ConfigureAwait(false)).Trim();
            var errors = (await errorTask.ConfigureAwait(false)).Trim();

            if (process.ExitCode == 0)
                return new Success<string>(output.Length > 0 ? output : errors);

            return new Error(new Exception(
                $"Exit code {process.ExitCode} from '{command}': {(errors.Length > 0 ? errors : output)}"));
        }
        catch (Exception ex)
        {
            return new Error(ex);
        }
    }

    /// <summary>
    /// Is this executable resolvable on PATH?
    /// </summary>
    public static bool Exists(string program)
    {
        var locator = System.OperatingSystem.IsWindows() ? "where" : "which";
        return Run(TerminalCommand.Raw(locator, program)).Match(_ => true, _ => false);
    }

}