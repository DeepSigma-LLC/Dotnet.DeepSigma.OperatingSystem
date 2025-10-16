using System.Runtime.InteropServices;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// A utility class to create temporary directories in the user's Downloads folder.
/// </summary>
public class TempFolderUtility
{
    private static readonly Guid KnownFolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
    /// <summary>
    /// Creats a temporary directory in the users downloads directory.
    /// Exception is thrown if your system is unable to locate the Downloads folder.
    /// </summary>
    /// <param name="prefix"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static DirectoryInfo CreateTempDirectoryInDownloads(string? prefix = null)
    {
        string? downloadsPath = GetDownloadsPath() ?? throw new InvalidOperationException("Unable to locate Downloads folder.");

        string tempFolderName = (prefix ?? String.Empty) + Guid.NewGuid().ToString();
        string fullPath = Path.Combine(downloadsPath, tempFolderName);
        DirectoryInfo directoryInfo = Directory.CreateDirectory(fullPath);
        return directoryInfo;
    }

    /// <summary>
    /// Use KnownFolders GUID to get Downloads directory path.
    /// </summary>
    /// <returns></returns>
    public static string? GetDownloadsPath()
    {
        int hr = SHGetKnownFolderPath(KnownFolderDownloads, 0, IntPtr.Zero, out nint outPath);

        if (hr != 0) return null;

        string? path = Marshal.PtrToStringUni(outPath);
        Marshal.FreeCoTaskMem(outPath);
        return path;
    }

    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath(
    [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
    uint dwFlags,
    IntPtr hToken,
     out IntPtr ppszPath);

}
