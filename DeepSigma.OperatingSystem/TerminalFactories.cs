using System.Collections.Immutable;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// Provides methods to run commands in a shell environment, such as cmd.exe on Windows or /bin/sh on Unix-like systems.
/// This class is useful for executing shell built-in commands, handling pipes, and performing redirection.
/// </summary>
public static class Shell
{
    /// <summary>
    /// Runs a command line through the system shell. Use for builtins (echo, dir), pipes, redirection.
    /// </summary>
    /// <param name="commandLine"></param>
    /// <returns></returns>
    public static TerminalCommand Run(string commandLine) => System.OperatingSystem.IsWindows()
        // /d skips AutoRun registry hooks, /s normalizes quote handling, /c runs and exits.
        ? new TerminalCommand { FileName = "cmd.exe", Arguments = ["/d", "/s", "/c", commandLine] }
        : new TerminalCommand { FileName = "/bin/sh", Arguments = ["-c", commandLine] };
}

/// <summary>
/// Powershell command line execution. Use for builtins (echo, dir), pipes, redirection.
/// </summary>
public static class PowerShell
{
    // pwsh (7+) if present, else Windows PowerShell 5.1.
    private static string Exe => Terminal.Exists("pwsh") ? "pwsh" : "powershell";

    /// <summary>
    /// Runs a command line through PowerShell. Use for builtins (echo, dir), pipes, redirection.
    /// </summary>
    /// <param name="script"></param>
    /// <returns></returns>
    public static TerminalCommand Run(string script) => new()
    {
        FileName = Exe,
        // -NoProfile: don't execute the user's profile (slow, and can print junk into your stdout).
        // -NonInteractive: fail instead of prompting, which would otherwise hang forever.
        // -Command: without it, PowerShell opens a REPL and ignores you.
        Arguments = ["-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-Command", script],
    };

    /// <summary>
    /// Runs a PowerShell script file with the specified arguments. Use for builtins (echo, dir), pipes, redirection.
    /// </summary>
    /// <param name="scriptPath"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static TerminalCommand File(string scriptPath, params string[] args) => new()
    {
        FileName = Exe,
        Arguments = ["-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", scriptPath, .. args],
    };
}

/// <summary>
/// Python  class to execute Python scripts and manage Python environments.
/// </summary>
public static class Python
{
    // 'python' is a Store stub on Windows and often absent on Linux. 'py' / 'python3' are correct.
    private static string Exe => System.OperatingSystem.IsWindows() ? "py" : "python3";

    private static readonly ImmutableDictionary<string, string> RequiredEnv =
        ImmutableDictionary<string, string>.Empty
            .Add("PYTHONUNBUFFERED", "1")     // stdout is block-buffered on a pipe; without this you get nothing until exit
            .Add("PYTHONIOENCODING", "utf-8"); // otherwise Windows emits cp1252 and mangles non-ASCII

    private static TerminalCommand Base(string exe, IEnumerable<string> args) =>
        new() { FileName = exe, Arguments = [.. args], Environment = RequiredEnv, StandardErrorIsInformational = true };

    /// <summary>
    /// Run a Python script with the specified arguments.
    /// </summary>
    /// <param name="path">The path to the Python script.</param>
    /// <param name="args">Arguments to pass to the script.</param>
    /// <returns>A <see cref="TerminalCommand"/> representing the script execution.</returns>
    public static TerminalCommand Script(string path, params string[] args) => Base(Exe, [path, .. args]);

    /// <summary>
    /// Run inline Python code with the specified arguments.
    /// </summary>
    /// <param name="code">The Python code to execute.</param>
    /// <returns>A <see cref="TerminalCommand"/> representing the inline code execution.</returns>
    public static TerminalCommand Inline(string code) => Base(Exe, ["-c", code]);

    /// <summary>
    /// Run a Python module with the specified arguments.
    /// </summary>
    /// <param name="module">The name of the Python module to run.</param>
    /// <param name="args">Arguments to pass to the module.</param>
    /// <returns>A <see cref="TerminalCommand"/> representing the module execution.</returns>
    public static TerminalCommand Module(string module, params string[] args) => Base(Exe, ["-m", module, .. args]);

    /// <summary>
    /// Run the pip package manager with the specified arguments.
    /// </summary>
    /// <param name="args">Arguments to pass to pip.</param>
    /// <returns>A <see cref="TerminalCommand"/> representing the pip execution.</returns>
    public static TerminalCommand Pip(params string[] args) => Module("pip", args);

    /// <summary>
    /// Creates a Python virtual environment wrapper.
    /// <remarks>
    /// A venv is NOT activated by inheriting PATH — activation is a shell-session property.
    /// The only reliable way is to invoke the venv's interpreter directly.
    /// </remarks>
    /// </summary>
    public static PythonEnvironment VirtualEnv(string venvPath) => new(venvPath);

    /// <summary>
    /// Represents a Python virtual environment, allowing you to run Python scripts, modules, and pip commands within that environment.
    /// </summary>
    /// <param name="venvPath">The path to the virtual environment.</param>
    public sealed class PythonEnvironment(string venvPath)
    {
        private string Exe => Path.Combine(venvPath,
            System.OperatingSystem.IsWindows() ? @"Scripts\python.exe" : "bin/python");

        /// <summary>
        /// Run a Python script within the virtual environment with the specified arguments.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public TerminalCommand Script(string path, params string[] args) => Base(Exe, [path, .. args]);

        /// <summary>
        /// Run inline Python code within the virtual environment with the specified arguments.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public TerminalCommand Module(string module, params string[] args) => Base(Exe, ["-m", module, .. args]);

        /// <summary>
        /// Run the pip package manager within the virtual environment with the specified arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public TerminalCommand Pip(params string[] args) => Module("pip", args);
    }
}

/// <summary>
/// Node.js 'node' and 'npm' commands.
/// </summary>
public static class Node
{
    /// <summary>
    /// Runs a Node.js script with the specified arguments.
    /// </summary>
    /// <param name="file">The path to the Node.js script file.</param>
    /// <param name="args">Arguments to pass to the script.</param>
    /// <returns>A <see cref="TerminalCommand"/> representing the script execution.</returns>
    public static TerminalCommand Script(string file, params string[] args) =>
        new() { FileName = "node", Arguments = [file, .. args] };

    /// <summary>
    /// Runs the npm package manager with the specified arguments.
    /// </summary>
    /// <param name="args">Arguments to pass to npm.</param>
    /// <returns>A <see cref="TerminalCommand"/> representing the npm execution.</returns>
    public static TerminalCommand Npm(params string[] args) => Shim("npm", args);

    /// <summary>
    /// Runs the yarn package manager with the specified arguments.
    /// </summary>
    /// <param name="args">Arguments to pass to yarn.</param>
    /// <returns>A <see cref="TerminalCommand"/> representing the yarn execution.</returns>
    public static TerminalCommand Yarn(params string[] args) => Shim("yarn", args);

    private static TerminalCommand Shim(string tool, string[] args)
    {
        var line = $"{tool} {string.Join(' ', args.Select(Quote))}";
        return Shell.Run(line) with { StandardErrorIsInformational = true };
    }

    private static string Quote(string a) => a.Contains(' ') ? $"\"{a}\"" : a;
}

/// <summary>
/// Git commands. Git is not a shell, so you can run it directly without going through cmd.exe or sh.
/// </summary>
public static class GitCommand
{
    /// <summary>
    /// Runs a Git command with the specified arguments.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static TerminalCommand Run(params string[] args) => new()
    {
        FileName = "git",
        // --no-pager: git will otherwise try to page and hang against a pipe.
        Arguments = ["--no-pager", .. args],
        StandardErrorIsInformational = true, // 'git clone' writes progress to stderr on SUCCESS
    };
}

/// <summary>
/// Dotnet CLI commands. The .NET SDK is not a shell, so you can run it directly without going through cmd.exe or sh.
/// </summary>
public static class Dotnet
{
    /// <summary>
    /// Runs a dotnet command with the specified arguments.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static TerminalCommand Run(params string[] args) =>
        new() { FileName = "dotnet", Arguments = [.. args], StandardErrorIsInformational = true };

    /// <summary>
    /// Builds a .NET project. This is a convenience method that wraps the 'dotnet build' command with the specified project path and the '--nologo' option to suppress the logo output.
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static TerminalCommand Build(string project) => Run("build", project, "--nologo");

    /// <summary>
    /// Tests a .NET project. This is a convenience method that wraps the 'dotnet test' command with the specified project path and the '--nologo' option to suppress the logo output.
    /// </summary>
    /// <param name="project"></param>
    /// <returns></returns>
    public static TerminalCommand Test(string project) => Run("test", project, "--nologo");
}

// ---------------------------------------------------------------------------
// DESCRIPTOR CHANGE
// Add this to TerminalCommand. 'dotnet run -' needs source piped to stdin,
// but the runner currently closes stdin immediately to prevent hangs.
// Making it an explicit, opt-in property keeps the safe default intact.
// ---------------------------------------------------------------------------
//
//     public string? StandardInput { get; init; }
//
// And in Terminal.RunAsync, replace  process.StandardInput.Close();  with:
//
//     if (command.StandardInput is { } stdin)
//         await process.StandardInput.WriteAsync(stdin.AsMemory(), ct).ConfigureAwait(false);
//     process.StandardInput.Close();   // ALWAYS close — EOF is what unblocks the child.
//
// ---------------------------------------------------------------------------

/// <summary>
/// .NET 10 file-based apps ("dotnet run app.cs"). Single .cs file, no .csproj,
/// NuGet via #:package directives.
/// </summary>
public static class DotnetScript
{
    private static readonly ImmutableDictionary<string, string> RequiredEnv =
        ImmutableDictionary<string, string>.Empty
            .Add("DOTNET_NOLOGO", "1")
            .Add("DOTNET_CLI_TELEMETRY_OPTOUT", "1");

    // First run = NuGet restore + compile. Can easily take 10-30s; longer with packages.
    private static readonly TimeSpan ColdStartTimeout = TimeSpan.FromMinutes(5);

    private static TerminalCommand Dotnet(IEnumerable<string> args, TimeSpan? timeout = null) => new()
    {
        FileName = "dotnet",
        Arguments = [.. args],
        Environment = RequiredEnv,
        StandardErrorIsInformational = true, // restore/build progress goes to stderr on SUCCESS
        Timeout = timeout ?? ColdStartTimeout,
    };

    /// <summary>
    /// Runs a .cs file as a file-based app.
    ///
    /// The '--file' flag is NOT optional and NOT cosmetic. Without it, if the working
    /// directory contains a .csproj, dotnet silently runs THAT PROJECT instead and passes
    /// your script path to it as argv[0]. It exits 0. You get the wrong program's output
    /// and no indication anything went wrong. This is the single most important reason
    /// callers must never assemble this invocation themselves.
    /// </summary>
    /// <param name="scriptPath">The path to the script file to be executed.</param>
    /// <param name="scriptArgs">The arguments to be passed to the script.</param>
    /// <returns>A TerminalCommand configured to run the specified script.</returns>
    public static TerminalCommand Run(string scriptPath, params string[] scriptArgs)
    {
        List<string> args = ["run", "--file", scriptPath];

        // '--' separates dotnet's own args from the script's args. Forget it and
        // 'dotnet run --file s.cs --verbose' means dotnet is verbose, not your script.
        if (scriptArgs.Length > 0)
        {
            args.Add("--");
            args.AddRange(scriptArgs);
        }

        return Dotnet(args);
    }

    /// <summary>
    /// Same as Run, but skips the implicit NuGet restore. Only safe when the script's
    /// #:package directives haven't changed since the last successful run.
    /// </summary>
    /// <param name="scriptPath">The path to the script file to be executed.</param>
    /// <param name="scriptArgs">The arguments to be passed to the script.</param>
    /// <returns>A TerminalCommand configured to run the specified script with caching enabled.</returns>
    public static TerminalCommand RunCached(string scriptPath, params string[] scriptArgs)
    {
        var command = Run(scriptPath, scriptArgs);
        return command with
        {
            Arguments = command.Arguments.Insert(1, "--no-restore"),
            Timeout = TimeSpan.FromMinutes(1),
        };
    }

    /// <summary>
    /// Compiles and runs C# source piped straight to stdin — nothing touches disk.
    /// Useful for executing generated code. Note this REQUIRES StandardInput support
    /// in the runner; the source is the process's stdin, so the script itself cannot
    /// read from stdin.
    /// </summary>
    /// <param name="sourceCode">The C# source code to be executed.</param>
    /// <returns>A TerminalCommand configured to run the specified source code.</returns>   
    public static TerminalCommand RunSource(string sourceCode) =>
        Dotnet(["run", "-"]) with { StandardInput = sourceCode };

    /// <summary>
    /// Builds a .NET project from a specified script file. This is useful for scenarios where you want to compile the script into an executable or library without running it immediately.
    /// </summary>
    /// <param name="scriptPath">The path to the script file to be built.</param>
    /// <returns>A TerminalCommand configured to build the specified script.</returns>
    public static TerminalCommand Build(string scriptPath) =>
        Dotnet(["build", "--file", scriptPath]);

    /// <summary>
    /// Converts a file-based script into a .NET project. This is useful for scenarios where you want to transition from a single-file script to a more structured project format, allowing for better organization and management of dependencies.
    /// </summary>
    /// <param name="scriptPath">The path to the script file to be converted.</param>
    /// <returns>A TerminalCommand configured to convert the specified script into a .NET project.</returns>
    public static TerminalCommand ConvertToProject(string scriptPath) =>
        Dotnet(["project", "convert", scriptPath]);

    /// <summary>
    /// Cleans up the local NuGet cache of artifacts that haven't been used for a specified number of days. This helps free up disk space by removing old and unused packages.
    /// </summary>
    /// <param name="unusedForDays">The number of days an artifact must be unused to be considered for cleanup.</param>
    /// <returns>A TerminalCommand configured to clean up the specified artifacts.</returns>
    public static TerminalCommand CleanArtifacts(int unusedForDays = 30) =>
        Dotnet(["clean", "--days", unusedForDays.ToString()]);

}
