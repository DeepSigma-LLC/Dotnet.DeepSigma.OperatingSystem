using DeepSigma.Core.Monads;
using System;
using System.Collections.Concurrent;
namespace DeepSigma.OperatingSystem;

/// <summary>
/// Resolves an executable name to its full path by walking PATH directly.
/// No subprocess, no encoding issues, and it correctly rejects Windows Store alias stubs
/// that 'where.exe' reports as valid.
/// </summary>
public static class ProgramLocator
{
    private static readonly ConcurrentDictionary<string, string?> Cache =
        new(System.OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

    /// <summary>Full path to the executable, or an Error if it isn't on PATH.</summary>
    public static ResultMonad<string> Locate(string program)
    {
        var resolved = Cache.GetOrAdd(program, static p => Search(p).FirstOrDefault());

        return resolved is not null
            ? new Success<string>(resolved)
            : new Error(new FileNotFoundException(
                $"'{program}' was not found on PATH.", program));
    }

    public static bool Exists(string program) => Locate(program).Match(_ => true, _ => false);

    /// <summary>
    /// EVERY match on PATH, in resolution order. The one that wins is [0].
    /// Use this when a tool is behaving strangely — it's usually a shadowing problem.
    /// </summary>
    public static IReadOnlyList<string> LocateAll(string program) => [.. Search(program)];

    /// <summary>First of the candidates that actually exists. For picking py vs python3 vs python.</summary>
    public static string? FirstAvailable(params string[] candidates) =>
        candidates.FirstOrDefault(Exists);

    /// <summary>The cache assumes PATH is stable. Call this if something gets installed mid-session.</summary>
    public static void ClearCache() => Cache.Clear();

    private static IEnumerable<string> Search(string program)
    {
        // If it already contains a separator it's a path, not a PATH lookup.
        if (program.Contains(Path.DirectorySeparatorChar) ||
            program.Contains(Path.AltDirectorySeparatorChar))
        {
            var direct = Path.GetFullPath(program);
            if (IsRunnable(direct)) yield return direct;
            yield break;
        }

        var directories = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var extensions = ExtensionsToTry(program);

        // Directory-major, extension-minor — this is the order Windows itself resolves in.
        foreach (var directory in directories)
        {
            foreach (var extension in extensions)
            {
                string candidate;
                try
                {
                    // PATH routinely contains junk entries with invalid path characters.
                    candidate = Path.Combine(directory.Trim('"'), program + extension);
                }
                catch (ArgumentException)
                {
                    break;
                }

                if (IsRunnable(candidate))
                    yield return candidate;
            }
        }
    }

    private static string[] ExtensionsToTry(string program)
    {
        if (!System.OperatingSystem.IsWindows()) return [string.Empty];
        if (Path.HasExtension(program)) return [string.Empty];

        // PATHEXT is why 'git' finds git.exe and 'npm' finds npm.cmd.
        var pathExt = Environment.GetEnvironmentVariable("PATHEXT") ?? ".COM;.EXE;.BAT;.CMD";
        return [.. pathExt.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }

    private static bool IsRunnable(string candidate)
    {
        FileInfo info;
        try { info = new FileInfo(candidate); }
        catch { return false; }

        if (!info.Exists) return false;

        if (System.OperatingSystem.IsWindows())
        {
            // Microsoft Store app-execution aliases (e.g. WindowsApps\python.exe) are
            // zero-byte reparse points. 'where.exe' reports them as hits; launching one
            // with UseShellExecute=false fails, or opens the Store. Treat them as absent.
            return info.Length > 0;
        }

        const UnixFileMode anyExecute =
            UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

        return (info.UnixFileMode & anyExecute) != 0;
    }
}
