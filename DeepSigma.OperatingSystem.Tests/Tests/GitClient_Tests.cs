using DeepSigma.Core.Monads;
using DeepSigma.OperatingSystem.FileSystem;
using DeepSigma.OperatingSystem.Git;
using Xunit;

namespace DeepSigma.OperatingSystem.Tests.Tests;

public class GitClient_Tests
{
    [Fact]
    public async Task GitClient_ExecutesCommandsWithSeparateArguments()
    {
        string root = Path.Combine(Path.GetTempPath(), $"deepsigma-git-client-{Guid.NewGuid():N}");
        string source = Path.Combine(root, "source repo");
        string remote = Path.Combine(root, "remote repo.git");
        string first = Path.Combine(root, "first clone");
        string second = Path.Combine(root, "second clone");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(first);
        Directory.CreateDirectory(second);

        try
        {
            var sourceClient = new GitClient(source);
            AssertSuccess(await sourceClient.InitAsync());
            await ConfigureRepository(source);

            const string firstFile = "-file with spaces.txt";
            await File.WriteAllTextAsync(Path.Combine(source, firstFile), "first version");
            AssertSuccess(await sourceClient.AddFileAsync(firstFile));
            Assert.Equal(firstFile, await RunGit(source, "diff", "--cached", "--name-only"));

            const string message = "Initial \"quoted\" message";
            AssertSuccess(await sourceClient.CommitAsync(message));
            Assert.Equal(message, await RunGit(source, "log", "-1", "--format=%s"));

            await RunGit(root, "clone", "--bare", "--", source, remote);
            var firstClient = new GitClient(first);
            var secondClient = new GitClient(second);
            AssertSuccess(await firstClient.CloneAsync(remote));
            AssertSuccess(await secondClient.CloneAsync(remote));
            await ConfigureRepository(first);
            await RunGit(first, "config", "push.default", "simple");

            await File.WriteAllTextAsync(Path.Combine(first, firstFile), "second version");
            await File.WriteAllTextAsync(Path.Combine(first, "another file.txt"), "another file");
            AssertSuccess(await firstClient.AddAllAsync());
            AssertSuccess(await firstClient.CommitAsync("Second commit"));

            var log = await firstClient.LogAsync(1);
            AssertSuccess(log);
            Assert.Contains("Second commit", log.Output);
            Assert.DoesNotContain(message, log.Output);

            AssertSuccess(await firstClient.PushAsync());
            AssertSuccess(await secondClient.PullAsync());
            Assert.Equal("second version", await File.ReadAllTextAsync(Path.Combine(second, firstFile)));
            Assert.Equal("another file", await File.ReadAllTextAsync(Path.Combine(second, "another file.txt")));
        }
        finally
        {
            FileSystemUtilities.ForceDeleteAllFilesAndDirectoriesRecursively(root);
        }
    }

    [Fact]
    public async Task CloneAsync_ReturnsErrorForMissingRepository()
    {
        string root = Path.Combine(Path.GetTempPath(), $"deepsigma-git-client-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var result = await new GitClient(root).CloneAsync(Path.Combine(root, "missing repo"));

            Assert.False(result.Success);
            Assert.NotEmpty(result.Error);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task ConfigureRepository(string path)
    {
        await RunGit(path, "config", "user.name", "DeepSigma Tests");
        await RunGit(path, "config", "user.email", "test@example.invalid");
        await RunGit(path, "config", "commit.gpgsign", "false");
    }

    private static async Task<string> RunGit(string path, params string[] arguments)
    {
        var result = await Terminal.RunAsync(GitCommand.Run(arguments).In(path));
        return result switch
        {
            SuccessResult<string> success => success.Result ?? string.Empty,
            ErrorResult error => throw error.Exception,
            _ => throw new InvalidOperationException("Unexpected Git result.")
        };
    }

    private static void AssertSuccess(GitResult result)
    {
        Assert.True(result.Success, result.Error);
        Assert.Equal(0, result.ExitCode);
    }
}
