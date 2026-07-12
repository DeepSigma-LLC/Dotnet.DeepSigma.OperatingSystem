using System.Collections.Immutable;
using System.Text;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// Represents a command to be executed in a terminal, including the executable file name, arguments, environment variables, working directory, standard input, and timeout.
/// </summary>
public sealed record TerminalCommand
{
    /// <summary>
    /// The name of the executable file to run. This is a required property and must be set when creating an instance of TerminalCommand.
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Argument LIST, not a string. Each element is passed verbatim — no quoting bugs.
    /// </summary>
    public ImmutableArray<string> Arguments { get; init; } = [];

    /// <summary>
    /// Environment variables to set for the child process. This is an immutable dictionary where each key-value pair represents an environment variable name and its corresponding value.
    /// </summary>
    public ImmutableDictionary<string, string> Environment { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// The working directory for the child process. If not set, the current working directory of the parent process will be used. This property is optional and can be null.
    /// </summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// Text written to the child's stdin before the pipe is closed. Null means "write nothing",
    /// which is the safe default. The pipe is ALWAYS closed either way — the EOF is what stops
    /// a child from blocking forever waiting on input it will never get.
    /// </summary>
    public string? StandardInput { get; init; }

    /// <summary>
    /// The maximum amount of time to wait for the command to complete before timing out. The default value is 5 minutes. This property is optional and can be set to a different TimeSpan value as needed.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// True for tools like git/npm/pip that write normal progress text to stderr on success.
    /// Tells the runner not to treat stderr content as evidence of failure.
    /// </summary>
    public bool StandardErrorIsInformational { get; init; }

    // -- fluent options; keep tool knowledge separate from call-site preferences --

    /// <summary>
    /// Sets the working directory for the command. This method returns a new instance of TerminalCommand with the specified working directory, allowing for fluent configuration of the command.
    /// </summary>
    /// <param name="workingDirectory"></param>
    /// <returns></returns>
    public TerminalCommand In(string workingDirectory) => this with { WorkingDirectory = workingDirectory };

    /// <summary>
    /// Sets the timeout for the command. This method returns a new instance of TerminalCommand with the specified timeout, allowing for fluent configuration of the command.
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public TerminalCommand WithTimeout(TimeSpan timeout) => this with { Timeout = timeout };

    /// <summary>
    /// Sets an environment variable for the command. This method returns a new instance of TerminalCommand with the specified environment variable, allowing for fluent configuration of the command.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public TerminalCommand WithEnvironment(string key, string value) =>
        this with { Environment = Environment.SetItem(key, value) };

    /// <summary>
    /// Sets the standard input for the command. This method returns a new instance of TerminalCommand with the specified standard input, allowing for fluent configuration of the command.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public TerminalCommand WithStandardInput(string text) =>
        this with { StandardInput = text };

    /// <summary>
    /// Creates a TerminalCommand instance with the specified file name and arguments. 
    /// This method is intended for advanced scenarios where you need to bypass the default behavior of the command execution. 
    /// Use this method with caution, as it allows you to specify any executable and arguments, which may lead to unexpected behavior if not used correctly.
    /// <remarks>
    /// ESCAPE HATCH. You own every quirk: shims, required flags, env vars, buffering.
    /// Named so it is greppable in review. Prefer a factory; if you find yourself
    /// calling Raw twice for the same tool, write a factory instead.
    /// </remarks>
    /// </summary>
    public static TerminalCommand Raw(string fileName, params string[] arguments)
    {
        // Guard the two mistakes that are guaranteed to happen otherwise.
        if (IsShellBinary(fileName) && !arguments.Any(a => a is "/c" or "/k" or "-c" or "-Command"))
        {
            throw new ArgumentException(
                $"'{fileName}' is a shell and will sit at an interactive prompt without /c (cmd) " +
                $"or -Command (PowerShell). Use Shell.Run(...) or PowerShell.Run(...) instead.",
                nameof(arguments));
        }

        if (System.OperatingSystem.IsWindows() && fileName.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"'{fileName}' is a batch shim; CreateProcess cannot execute it directly. " +
                $"Route it through Shell.Run(...).",
                nameof(fileName));
        }

        return new TerminalCommand { FileName = fileName, Arguments = [.. arguments] };
    }

    private static bool IsShellBinary(string f) =>
        Path.GetFileNameWithoutExtension(f).ToLowerInvariant() is "cmd" or "powershell" or "pwsh" or "sh" or "bash";

    /// <summary>
    /// Loggable, and assertable in a unit test without ever spawning a process.
    /// </summary>
    public override string ToString() => $"{FileName} {string.Join(' ', Arguments)}";
}
