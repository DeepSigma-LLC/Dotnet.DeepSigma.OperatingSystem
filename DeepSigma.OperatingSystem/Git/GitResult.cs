namespace DeepSigma.OperatingSystem.Git;

/// <summary>
/// Represents the result of a Git command execution, including success status, output, error message, and exit code.
/// </summary>
/// <param name="Success">true if the Git command completed successfully; otherwise, false.</param>
/// <param name="Output">The standard output produced by the Git command. This value is trimmed and is empty if the command failed.</param>
/// <param name="Error">The error output produced by the Git command. This value is trimmed and is empty if the command succeeded.</param>
/// <param name="ExitCode">The exit code returned by the Git process. A value of 0 typically indicates success.</param>
public record GitResult(
    bool Success,
    string Output,
    string Error,
    int ExitCode
)
{
    /// <summary>
    /// Creates a successful GitResult with the specified output.
    /// </summary>
    /// <param name="output">The standard output from the Git command. Leading and trailing whitespace is removed.</param>
    /// <returns>A GitResult instance representing a successful operation with the provided output.</returns>
    public static GitResult Ok(string output) =>
        new(true, output.Trim(), string.Empty, 0);


    /// <summary>
    /// Creates a failed <see cref="GitResult"/> instance with the specified error message and exit code.
    /// </summary>
    /// <param name="error">The error message describing the reason for the failure. Leading and trailing whitespace is removed.</param>
    /// <param name="exitCode">The exit code returned by the failed operation.</param>
    /// <returns>A <see cref="GitResult"/> representing a failed operation, containing the provided error message and exit code.</returns>
    public static GitResult Fail(string error, int exitCode) =>
        new(false, string.Empty, error.Trim(), exitCode);
}