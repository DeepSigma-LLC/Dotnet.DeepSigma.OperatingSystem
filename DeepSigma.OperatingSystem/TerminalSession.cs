using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DeepSigma.OperatingSystem;


/// <summary>
/// A class to manage processes on the local machine.
/// </summary>
/// <param name="StandardOutput"></param>
/// <param name="StandardError"></param>
/// <param name="ExitCode"></param>
public sealed record CommandResult(string StandardOutput, string StandardError, int ExitCode)
{
    /// <summary>
    /// Indicates whether the command executed successfully based on the exit code.
    /// </summary>
    public bool Succeeded => ExitCode == 0;
}

/// <summary>
/// A long-lived shell process you can send commands to repeatedly.
/// Output is streamed via <see cref="OutputReceived"/> / <see cref="ErrorReceived"/>,
/// and <see cref="ExecuteAsync"/> gives request/response semantics on top of that stream.
/// </summary>
public sealed class TerminalSession : IAsyncDisposable
{
    // stderr carries no completion marker, so we let it settle briefly after stdout finishes.
    private const int StderrSettleMs = 50;

    private readonly Process _process;
    private readonly StreamWriter _stdin;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly SemaphoreSlim _oneCommandAtATime = new(1, 1);
    private readonly Task _stdoutPump;
    private readonly Task _stderrPump;
    private readonly string _promptToken;
    private readonly bool _isWindows;
    private readonly object _gate = new();

    private readonly StringBuilder _rawStdout = new();
    private readonly StringBuilder _rawStderr = new();

    // Carry-over so a prompt token split across two reads still gets stripped.
    private string _stdoutCarry = string.Empty;

    private TaskCompletionSource<bool>? _pendingMarker;
    private string? _pendingMarkerText;

    /// <summary>
    /// Raw stdout chunks, prompt noise removed. Fires on a thread-pool thread.
    /// </summary>
    public event Action<string>? OutputReceived;

    /// <summary>
    /// Raw stderr chunks. Fires on a thread-pool thread.
    /// </summary>
    public event Action<string>? ErrorReceived;

    /// <summary>Raised when the shell process terminates for any reason.</summary>
    public event Action<int>? Exited;

    /// <summary>
    /// Indicates whether the shell process is still running. If false, the process has exited and no further commands can be sent.
    /// </summary>
    public bool IsRunning => !_process.HasExited;

    private TerminalSession(Process process, string promptToken, bool isWindows)
    {
        _process = process;
        _promptToken = promptToken;
        _isWindows = isWindows;
        _stdin = process.StandardInput;
        _stdin.AutoFlush = false;

        _stdoutPump = Task.Run(() => PumpAsync(process.StandardOutput, isError: false));
        _stderrPump = Task.Run(() => PumpAsync(process.StandardError, isError: true));

        process.Exited += (_, _) =>
        {
            // Never leave a caller awaiting a marker that can no longer arrive.
            lock (_gate)
            {
                _pendingMarker?.TrySetException(
                    new InvalidOperationException($"Shell exited (code {_process.ExitCode}) before the command completed."));
            }
            Exited?.Invoke(_process.ExitCode);
        };
    }

    /// <summary>
    /// Starts a new shell process and returns a <see cref="TerminalSession"/> to manage it.
    /// </summary>
    /// <param name="workingDirectory"></param>
    /// <returns></returns>
    public static TerminalSession Start(string? workingDirectory = null)
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        string token = $"__P{Guid.NewGuid():N}__";

        var psi = new ProcessStartInfo
        {
            FileName = isWindows ? "cmd.exe" : "/bin/bash",

            // Windows: /q suppresses command echo, /k keeps the shell alive,
            //          and we set the prompt to a token we can strip out reliably.
            // Unix:    -s reads commands from the stdin pipe non-interactively,
            //          which emits no prompt and no echo at all.
            Arguments = isWindows ? $"/q /k prompt {token}" : "-s",

            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        if (!string.IsNullOrEmpty(workingDirectory))
            psi.WorkingDirectory = workingDirectory;

        // Any Python launched inside this shell inherits these.
        psi.EnvironmentVariables["PYTHONUNBUFFERED"] = "1";
        psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

        var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        process.Start();

        var session = new TerminalSession(process, token, isWindows);

        if (isWindows)
        {
            // cmd defaults to an OEM codepage; without this, StandardOutputEncoding=UTF8 mangles non-ASCII.
            session.SendRawAsync("chcp 65001 > nul").GetAwaiter().GetResult();
        }

        return session;
    }

    /// <summary>
    /// Sends a command and waits for it to finish. Safe to call concurrently — calls are serialized.
    /// </summary>
    public async Task<CommandResult> ExecuteAsync(string command, CancellationToken ct = default)
    {
        if (_process.HasExited)
            throw new InvalidOperationException("The shell session has already exited.");

        await _oneCommandAtATime.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var marker = $"__END_{Guid.NewGuid():N}__";
            var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            lock (_gate)
            {
                _rawStdout.Clear();
                _rawStderr.Clear();
                _pendingMarkerText = marker;
                _pendingMarker = completed;
            }

            await _stdin.WriteLineAsync(command).ConfigureAwait(false);
            await _stdin.WriteLineAsync(_isWindows
                ? $"echo {marker} %ERRORLEVEL%"
                : $"echo {marker} $?").ConfigureAwait(false);
            await _stdin.FlushAsync(ct).ConfigureAwait(false);

            await using (ct.Register(() => completed.TrySetCanceled(ct)).ConfigureAwait(false))
            {
                await completed.Task.ConfigureAwait(false);
            }

            await Task.Delay(StderrSettleMs, ct).ConfigureAwait(false);

            lock (_gate)
            {
                _pendingMarker = null;
                _pendingMarkerText = null;

                string raw = _rawStdout.ToString();
                int idx = raw.IndexOf(marker, StringComparison.Ordinal);

                string body = idx >= 0 ? raw[..idx] : raw;
                string tail = idx >= 0 ? raw[(idx + marker.Length)..] : string.Empty;

                int exitCode = int.TryParse(
                    tail.Trim().Split('\n', 2)[0].Trim(), out int parsed) ? parsed : -1;

                return new CommandResult(Strip(body).Trim(), _rawStderr.ToString().Trim(), exitCode);
            }
        }
        finally
        {
            _oneCommandAtATime.Release();
        }
    }

    /// <summary>
    /// Writes straight to stdin with no marker handshake. Use this to drive a REPL
    /// (python -i, dotnet fsi) where you're watching the stream yourself.
    /// </summary>
    public async Task SendRawAsync(string text, bool appendNewline = true, CancellationToken ct = default)
    {
        if (appendNewline)
            await _stdin.WriteLineAsync(text.AsMemory(), ct).ConfigureAwait(false);
        else
            await _stdin.WriteAsync(text.AsMemory(), ct).ConfigureAwait(false);

        await _stdin.FlushAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Waits until the given text appears in the stream. Needed for REPL prompts,
    /// which have no trailing newline and so never trip a line-based reader.
    /// Note: Python's REPL writes its ">>> " prompt to STDERR, not stdout.
    /// </summary>
    public async Task WaitForAsync(string needle, TimeSpan timeout, bool inStandardError = false, CancellationToken ct = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout);

        while (!cts.IsCancellationRequested)
        {
            lock (_gate)
            {
                var buf = inStandardError ? _rawStderr : _rawStdout;
                if (buf.ToString().Contains(needle, StringComparison.Ordinal))
                    return;
            }
            await Task.Delay(20, cts.Token).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timed out waiting for '{needle}'.");
    }

    private async Task PumpAsync(StreamReader reader, bool isError)
    {
        var buffer = new char[4096];
        try
        {
            while (true)
            {
                int read = await reader.ReadAsync(buffer.AsMemory(), _lifetime.Token).ConfigureAwait(false);
                if (read == 0) break; // EOF — the shell is gone.

                OnChunk(new string(buffer, 0, read), isError);
            }
        }
        catch (OperationCanceledException) { /* disposing */ }
        catch (IOException) { /* pipe torn down */ }
    }

    private void OnChunk(string chunk, bool isError)
    {
        string visible;

        lock (_gate)
        {
            if (isError)
            {
                _rawStderr.Append(chunk);
                visible = chunk;
            }
            else
            {
                _rawStdout.Append(chunk);

                // Emit everything except a tail that might be a half-received prompt token.
                string pending = _stdoutCarry + chunk;
                int safe = Math.Max(0, pending.Length - (_promptToken.Length - 1));
                visible = Strip(pending[..safe]);
                _stdoutCarry = pending[safe..];

                if (_pendingMarkerText is not null &&
                    _rawStdout.ToString().Contains(_pendingMarkerText, StringComparison.Ordinal))
                {
                    _pendingMarker?.TrySetResult(true);
                }
            }
        }

        if (visible.Length == 0) return;

        if (isError) ErrorReceived?.Invoke(visible);
        else OutputReceived?.Invoke(visible);
    }

    private string Strip(string s) => s.Replace(_promptToken, string.Empty);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!_process.HasExited)
            {
                await _stdin.WriteLineAsync("exit").ConfigureAwait(false);
                await _stdin.FlushAsync().ConfigureAwait(false);

                using var grace = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                try
                {
                    await _process.WaitForExitAsync(grace.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
        }
        catch { /* best effort */ }

        await _lifetime.CancelAsync().ConfigureAwait(false);
        try { await Task.WhenAll(_stdoutPump, _stderrPump).ConfigureAwait(false); } catch { }

        _process.Dispose();
        _lifetime.Dispose();
        _oneCommandAtATime.Dispose();
    }
}
