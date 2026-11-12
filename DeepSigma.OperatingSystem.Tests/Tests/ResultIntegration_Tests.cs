using System.Diagnostics;
using DeepSigma.Core.Monads;
using DeepSigma.OperatingSystem.Git;
using Xunit;

namespace DeepSigma.OperatingSystem.Tests.Tests;

public class ResultIntegration_Tests
{
    [Fact]
    public async Task Terminal_RunAsync_ReturnsOutputOnSuccess()
    {
        var result = await Terminal.RunAsync(Shell.Run("echo deepsigma-result"));

        var output = result switch
        {
            SuccessResult<string> success => success.Result,
            ErrorResult error => throw error.Exception,
            _ => throw new InvalidOperationException("Unexpected terminal result.")
        };
        Assert.Equal("deepsigma-result", output);
    }

    [Fact]
    public void Terminal_Run_ReturnsErrorOnNonzeroExit()
    {
        var result = Terminal.Run(Shell.Run("exit 17"));

        var exception = result switch
        {
            ErrorResult error => error.Exception,
            _ => throw new InvalidOperationException("Expected a terminal error.")
        };
        Assert.Contains("Exit code 17", exception.Message);
    }

    [Fact]
    public void Terminal_Run_ReturnsErrorWhenExecutableIsMissing()
    {
        var result = Terminal.Run(TerminalCommand.Raw($"deepsigma-missing-{Guid.NewGuid():N}"));

        Assert.True(result is ErrorResult);
    }

    [Fact]
    public void Terminal_Exists_ReportsPresentAndMissingPrograms()
    {
        Assert.True(Terminal.Exists("dotnet"));
        Assert.False(Terminal.Exists($"deepsigma-missing-{Guid.NewGuid():N}"));
    }

    [Fact]
    public void ProgramLocator_ReportsResolvedAndMissingPrograms()
    {
        using var process = Process.GetCurrentProcess();
        string executable = process.MainModule!.FileName;
        string missing = $"deepsigma-missing-{Guid.NewGuid():N}";
        var success = ProgramLocator.Locate(executable);
        var error = ProgramLocator.Locate(missing);

        Assert.True(success is SuccessResult<string> { Result: var path } && path == executable);
        Assert.True(error is ErrorResult { Exception: FileNotFoundException });
        Assert.True(ProgramLocator.Exists(executable));
        Assert.False(ProgramLocator.Exists(missing));
    }

    [Fact]
    public void ProcessManager_MultiErrorResultPreservesSuccessAndValidationFailure()
    {
        var success = ProcessManager.KillProcessByName($"deepsigma-missing-{Guid.NewGuid():N}");
        var failure = ProcessManager.KillProcessByName("");

        Assert.True(success is SuccessResult<int> { Result: 0 });
        var exceptions = failure switch
        {
            ErrorResults errors => errors.Exceptions,
            _ => throw new InvalidOperationException("Expected process validation errors.")
        };
        var exception = Assert.Single(exceptions);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void GitHelper_ClonesLocalRepositoryAndReportsFailure()
    {
        string root = Path.Combine(Path.GetTempPath(), $"deepsigma-git-{Guid.NewGuid():N}");
        string source = Path.Combine(root, "source repo");
        string target = Path.Combine(root, "target repo");
        Directory.CreateDirectory(root);
        try
        {
            var init = Terminal.Run(GitCommand.Run("init", source));
            if (init is ErrorResult error)
                throw error.Exception;
            Assert.True(init is SuccessResult<string>);

            Assert.Null(GitHelper.DownloadGitRepository(source, target));
            Assert.True(Directory.Exists(Path.Combine(target, ".git")));
            Assert.NotNull(GitHelper.DownloadGitRepository(Path.Combine(root, "missing"), Path.Combine(root, "failed")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
