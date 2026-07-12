using DeepSigma.Core.Monads;

namespace DeepSigma.OperatingSystem.Git;

internal class GitCommandRunner
{
    public async Task<GitResult> RunAsync(string arguments, string workingDirectory, CancellationToken ct = default)
    {
        var result = await Terminal.RunAsync(GitCommand.Run(arguments).In(workingDirectory), ct);

        return result.Match<GitResult>(
            success => GitResult.Ok(success.Result ?? string.Empty),
            error => GitResult.Fail(error.Exception.Message, 1)
        );
    }
}
