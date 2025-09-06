using DeepSigma.OperatingSystem.Enums;
using DeepSigma.OperatingSystem.Models;

namespace DeepSigma.OperatingSystem
{
    /// <summary>
    /// Represents a file system object, which can be either a file or a directory.
    /// </summary>
    public class FileSystemObject
    {
        public FileSystemType? Type { get; set; } = null;
        public string OriginalPath { get; set; } = string.Empty;
        public string DirectoryPath { get; set; } = string.Empty;
        public string DirectoryName { get; set; } = string.Empty;
        public FileProperties FileProperties { get; set; } = new();
        public FileSystemObject[] Files { get; set; } = [];
        public FileSystemObject[] Directories { get; set; } = [];

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemObject"/> class with the specified full file path.
        /// </summary>
        /// <param name="FullFilePath"></param>
        public FileSystemObject(string FullFilePath)
        {
            OriginalPath = FullFilePath;
            SetDirectory();
            SetDirectoryName();
            FileProperties.FileExtension = Path.GetExtension(FullFilePath);
            FileProperties.FileName = Path.GetFileName(FullFilePath);
            FileProperties.FileNameWithoutExtension = Path.GetFileNameWithoutExtension(FullFilePath);

            SetFileProperties();
            SetFileSystemType();

            if(Type.HasValue && Type.Value == FileSystemType.Directory)
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
            List<FileSystemObject> files = [];
            foreach (string file in Directory.GetFiles(DirectoryPath))
            {
                files.Add(new FileSystemObject(file));
            }
            Files = files.ToArray();
        }

        private void LoadDirectories()
        {
            List<FileSystemObject> directories = [];
            foreach (string directory in Directory.GetDirectories(DirectoryPath))
            {
                directories.Add(new FileSystemObject(directory));
            }
            Directories = directories.ToArray();
        }
        
        private void SetFileSystemType()
        {
            if (IsFile())
            {
                Type = FileSystemType.File;
            }
            else if(IsDirectory())
            {
                Type = FileSystemType.Directory;
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
}
