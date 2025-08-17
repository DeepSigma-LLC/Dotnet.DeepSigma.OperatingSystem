
namespace DeepSigma.OperatingSystem.Models
{
    /// <summary>
    /// Represents primary properties of a file, including its name, extension, creation time, last access time, and last write time.
    /// </summary>
    public class FileProperties
    {
        public string? FileName { get; set; } = null;
        public string? FileExtension { get; set; } = null;
        public string? FileNameWithoutExtension { get; set; } = null;
        public DateTime? CreationTimeUtc { get; set; } = null;
        public DateTime? LastAccessTimeUtc { get; set; } = null;
        public DateTime? LastWriteTimeUtc { get; set; } = null;
        public FileProperties() { }
    }
}
