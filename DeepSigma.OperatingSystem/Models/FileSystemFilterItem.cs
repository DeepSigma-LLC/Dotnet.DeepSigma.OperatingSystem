using DeepSigma.OperatingSystem.Enums;

namespace DeepSigma.OperatingSystem.Models
{
    /// <summary>
    /// Represents a file system filter item used for filtering files and directories.
    /// </summary>
    public class FileSystemFilterItem
    {
        /// <summary>
        /// The type of the file system object (File or Directory).
        /// </summary>
        public FileSystemType FileSystemType { get; set; }

        /// <summary>
        /// The original filter text as specified by the user.
        /// </summary>
        public string OriginalFilter { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the filter starts with a wildcard (e.g., '*text').
        /// </summary>
        public bool StartsWithAnything { get; set; } = false;

        /// <summary>
        /// Indicates if the filter ends with a wildcard (e.g., 'text*').
        /// </summary>
        public bool EndsWithAnything { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemFilterItem"/> class.
        /// </summary>
        /// <param name="original_text"></param>
        /// <param name="file_system_type"></param>
        /// <param name="starts_with_anything"></param>
        /// <param name="ends_with_anything"></param>
        public FileSystemFilterItem(string original_text, FileSystemType file_system_type, bool starts_with_anything = false, bool ends_with_anything = false)
        {
            FileSystemType = file_system_type;
            OriginalFilter = original_text;
            StartsWithAnything = starts_with_anything;
            EndsWithAnything = ends_with_anything;
        }
    }
}
