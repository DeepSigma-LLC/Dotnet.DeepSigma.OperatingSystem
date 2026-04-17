namespace DeepSigma.OperatingSystem.Git;


/// <summary>
/// A simple Git client that provides asynchronous methods for common Git operations such as initializing a repository, cloning, adding files, committing, logging, pulling, and pushing. 
/// This client uses a GitCommandRunner to execute Git commands in the specified repository path and returns results encapsulated in GitResult objects.
/// </summary>
public class GitClient
{
    private readonly GitCommandRunner _runner;
    private readonly string _repoPath;

    /// <summary>
    /// Initializes a new instance of the GitClient class for the specified repository path.
    /// </summary>
    /// <remarks>This constructor uses a default GitCommandRunner instance. Use this overload for standard Git
    /// operations without custom command execution logic.</remarks>
    /// <param name="repoPath">The file system path to the root directory of the Git repository. Cannot be null or empty.</param>
    public GitClient(string repoPath) : this(repoPath, new GitCommandRunner()) { }

    // Internal constructor for testing with a mock runner
    internal GitClient(string repoPath, GitCommandRunner runner)
    {
        _repoPath = repoPath;
        _runner = runner;
    }

    /// <summary>
    /// Initializes a new Git repository in the specified directory asynchronously.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a GitResult indicating the outcome
    /// of the initialization.</returns>
    public Task<GitResult> InitAsync(CancellationToken ct = default)
        => _runner.RunAsync("init", _repoPath, ct);

    /// <summary>
    /// Clones a Git repository from the specified remote URL into the current repository path asynchronously.
    /// </summary>
    /// <param name="remoteUrl">The URL of the remote Git repository to clone. Cannot be null or empty.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the clone operation.</param>
    /// <returns>A task that represents the asynchronous clone operation. The task result contains a GitResult indicating the
    /// outcome of the operation.</returns>
    public Task<GitResult> CloneAsync(string remoteUrl, CancellationToken ct = default)
        => _runner.RunAsync($"clone {remoteUrl} .", _repoPath, ct);

    /// <summary>
    /// Adds the specified file to the Git index asynchronously.
    /// </summary>
    /// <param name="filePath">The path to the file to add to the Git index. 
    /// The path must be relative to the repository root and cannot be null or empty.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a GitResult indicating the outcome
    /// of the add operation.
    /// </returns>
    public Task<GitResult> AddFileAsync(string filePath, CancellationToken ct = default)
        => _runner.RunAsync($"add \"{filePath}\"", _repoPath, ct);

    /// <summary>
    /// Stages all changes in the repository for the next commit asynchronously.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a GitResult indicating the outcome
    /// of the add operation.
    /// </returns>
    public Task<GitResult> AddAllAsync(CancellationToken ct = default)
        => _runner.RunAsync("add .", _repoPath, ct);

    /// <summary>
    /// Commits staged changes to the repository asynchronously using the specified commit message.
    /// </summary>
    /// <param name="message">The commit message to associate with the changes. Cannot be null or empty.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the commit operation.</param>
    /// <returns>
    /// A task that represents the asynchronous commit operation. The task result contains a GitResult indicating the
    /// outcome of the commit.
    /// </returns>
    public Task<GitResult> CommitAsync(string message, CancellationToken ct = default)
        => _runner.RunAsync($"commit -m \"{EscapeMessage(message)}\"", _repoPath, ct);

    /// <summary>
    /// Asynchronously retrieves a list of recent Git commit log entries for the repository.
    /// </summary>
    /// <param name="count">The maximum number of commit entries to retrieve. Must be greater than zero. The default is 10.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a GitResult with the commit log entries.
    /// </returns>
    public Task<GitResult> LogAsync(int count = 10, CancellationToken ct = default)
        => _runner.RunAsync($"log --oneline -n {count}", _repoPath, ct);

    /// <summary>
    /// Fetches and integrates changes from the remote repository into the current branch asynchronously.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the pull operation.</param>
    /// <returns>
    /// A task that represents the asynchronous pull operation. The task result contains a GitResult object with details
    /// about the pull process.
    /// </returns>
    public Task<GitResult> PullAsync(CancellationToken ct = default)
        => _runner.RunAsync("pull", _repoPath, ct);

    /// <summary>
    /// Pushes local commits to the remote Git repository asynchronously.
    /// </summary>
    /// <param name="ct">A cancellation token that can be used to cancel the push operation.</param>
    /// <returns>
    /// A task that represents the asynchronous push operation. The task result contains a GitResult indicating the
    /// outcome of the push.
    /// </returns>
    public Task<GitResult> PushAsync(CancellationToken ct = default)
        => _runner.RunAsync("push", _repoPath, ct);

    /// <summary>
    /// Escapes double quotes and replaces newline characters in the specified message string.
    /// </summary>
    /// <remarks>Use this method to prepare message strings for contexts where unescaped double quotes or
    /// newlines may cause formatting or parsing issues.</remarks>
    /// <param name="msg">The message string to be escaped. Cannot be null.</param>
    /// <returns>
    /// A string with all double quotes escaped and newline characters replaced with spaces.
    /// </returns>
    private static string EscapeMessage(string msg) =>
        msg.Replace("\"", "\\\"").Replace("\n", " ");
}