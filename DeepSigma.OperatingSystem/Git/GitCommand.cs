using System.Diagnostics;

namespace DeepSigma.OperatingSystem.Git;

internal class GitCommandRunner
{
    public async Task<GitResult> RunAsync(string arguments, string workingDirectory, CancellationToken ct = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(ct);
        var errorTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        var output = await outputTask;
        var error = await errorTask;

        return process.ExitCode == 0
            ? GitResult.Ok(output)
            : GitResult.Fail(error, process.ExitCode);
    }
}
