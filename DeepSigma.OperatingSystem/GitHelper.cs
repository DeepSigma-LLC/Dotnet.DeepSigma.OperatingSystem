using DeepSigma.General.Monads;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// Provides helper methods for Git operations.
/// </summary>
public static class GitHelper
{
    /// <summary>
    /// Downloads a Git repository to the specified target directory using the 'git clone' command.
    /// </summary>
    /// <param name="repositoryUrl"></param>
    /// <param name="targetDirectory"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static Exception? DownloadGitRepository(string repositoryUrl, string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
            return new ArgumentException("Repository URL cannot be null or empty.", nameof(repositoryUrl));
        if (string.IsNullOrWhiteSpace(targetDirectory))
            return new ArgumentException("Target directory cannot be null or empty.", nameof(targetDirectory));

        string gitCloneCommand = $"git clone {repositoryUrl} {targetDirectory}";

        ResultMonad<string> result = Terminal.RunCommand("cmd.exe", $"/C {gitCloneCommand}");

        Exception? outcome = null;
        result.Switch(
            success => outcome = null,
            error => outcome = error.Exception
        );

        return outcome;
    }
}
