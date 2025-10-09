using DeepSigma.OperatingSystem.Enums;
using DeepSigma.OperatingSystem.Models;

namespace DeepSigma.OperatingSystem;

/// <summary>
/// Represents a file system object, which can be either a file or a directory.
/// </summary>
public class FileSystemObject
{

    /// <summary>
    /// Type of file system object: File or Directory.
    /// </summary>
    public FileSystemType? ObjectSystemType { get; set; } = null;

    /// <summary>
    /// Original full path provided during initialization.
    /// </summary>
    public string OriginalPath { get; set; } = string.Empty;

    /// <summary>
    /// Directory path of the file or directory.
    /// </summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// Name of the directory.
    /// </summary>
    public string DirectoryName { get; set; } = string.Empty;

    /// <summary>
    /// Properties of the file, if the object is a file.
    /// </summary>
    public FileProperties FileProperties { get; set; } = new();

    /// <summary>
    /// Array of files contained within the directory, if the object is a directory.
    /// </summary>
    public FileSystemObject[] Files { get; set; } = [];

    /// <summary>
    /// Array of directories contained within the directory, if the object is a directory.
    /// </summary>
    public FileSystemObject[] Directories { get; set; } = [];
    private int MaxNumberOfLevelsToNavigateDown { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemObject"/> class with the specified full file path.
    /// </summary>
    /// <param name="FullFilePath">Full file path to load properties into the object.</param>
    /// <param name="max_number_of_levels_to_navigate_down">Sets a limit on the number of levels to recursively navigate down.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <remarks>
    /// <see cref="ArgumentOutOfRangeException"/> Thrown if max number of levels is negative.
    /// <see cref="ArgumentException"/>Thrown is file path cannot be null or empty.
    /// <see cref="FileNotFoundException"/>Thrown if an invalid file path is found.
    /// </remarks>
    public FileSystemObject(string FullFilePath, int max_number_of_levels_to_navigate_down)
    {
        if(max_number_of_levels_to_navigate_down < 0 )
        {
            throw new ArgumentOutOfRangeException(nameof(max_number_of_levels_to_navigate_down), "Value cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(FullFilePath))
        {
            throw new ArgumentException("Full file path cannot be null or empty.", nameof(FullFilePath));
        }

        if (!File.Exists(FullFilePath) && !Directory.Exists(FullFilePath))
        {
            throw new FileNotFoundException("The specified file or directory does not exist.", FullFilePath);
        }

        MaxNumberOfLevelsToNavigateDown = max_number_of_levels_to_navigate_down;
        OriginalPath = FullFilePath;
        SetDirectory();
        SetDirectoryName();
        FileProperties.FileExtension = Path.GetExtension(FullFilePath);
        FileProperties.FileName = Path.GetFileName(FullFilePath);
        FileProperties.FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FullFilePath);

        SetFileProperties();
        SetFileSystemType();

        if(ObjectSystemType.HasValue && ObjectSystemType.Value == FileSystemType.Directory)
        {
            LoadFiles();
            LoadDirectories();
        }
    }

    private void SetFileProperties()
    {
        if (IsFile())
        {
            FileProperties.CreationTimeUtc = File.GetCreationTimeUtc(OriginalPath);
            FileProperties.LastAccessTimeUtc = File.GetLastAccessTimeUtc(OriginalPath);
            FileProperties.LastWriteTimeUtc = File.GetLastWriteTimeUtc(OriginalPath);
        }
    }

    private void SetDirectory()
    {
        DirectoryPath = IsFile() ? (Path.GetDirectoryName(OriginalPath) ?? string.Empty) : OriginalPath;
    }

    private void SetDirectoryName()
    {
        DirectoryName = Path.GetFileName(DirectoryPath);
    }

    private void LoadFiles()
    {
        if (MaxNumberOfLevelsToNavigateDown == 0) { return; } // Stop recursion if limit is reached.

        List<FileSystemObject> files = [];
        foreach (string file in Directory.GetFiles(DirectoryPath))
        {
            files.Add(new FileSystemObject(file, MaxNumberOfLevelsToNavigateDown - 1));
        }
        Files = files.ToArray();
    }

    private void LoadDirectories()
    {
        if (MaxNumberOfLevelsToNavigateDown == 0) { return; } // Stop recursion if limit is reached.

        List<FileSystemObject> directories = [];
        foreach (string directory in Directory.GetDirectories(DirectoryPath))
        {
            directories.Add(new FileSystemObject(directory, MaxNumberOfLevelsToNavigateDown - 1));
        }
        Directories = directories.ToArray();
    }
    
    private void SetFileSystemType()
    {
        if (IsFile())
        {
            ObjectSystemType = FileSystemType.File;
        }
        else if(IsDirectory())
        {
            ObjectSystemType = FileSystemType.Directory;
        }
    }

    private bool IsFile()
    {
        if (File.Exists(OriginalPath))
        {
            return true;
        }
        return false;
    }

    private bool IsDirectory()
    {
        if (Directory.Exists(OriginalPath))
        {
            return true;
        }
        return false;
    }

}
