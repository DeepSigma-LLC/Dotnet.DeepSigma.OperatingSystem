
namespace DeepSigma.OperatingSystem.Models
{
    /// <summary>
    /// Represents primary properties of a file, including its name, extension, creation time, last access time, and last write time.
    /// </summary>
    public class FileProperties
    {
        /// <summary>
        /// The name of the file, including its extension.
        /// </summary>
        public string? FileName { get; set; } = null;

        /// <summary>
        /// The file extension, including the leading period (e.g., ".txt").
        /// </summary>
        public string? FileExtension { get; set; } = null;

        /// <summary>
        /// The name of the file without its extension.
        /// </summary>
        public string? FileNameWithoutExtension { get; set; } = null;

        /// <summary>
        /// The UTC creation time of the file. Null if not set.
        /// </summary>
        public DateTime? CreationTimeUtc { get; set; } = null;

        /// <summary>
        /// The UTC last access time of the file. Null if not set.
        /// </summary>
        public DateTime? LastAccessTimeUtc { get; set; } = null;

        /// <summary>
        /// The UTC last write time of the file. Null if not set.
        /// </summary>
        public DateTime? LastWriteTimeUtc { get; set; } = null;
    }
}
