using DeepSigma.OperatingSystem.Enums;

namespace DeepSigma.OperatingSystem.Models
{
    public class FileSystemFilterItem
    {
        public FileSystemType FileSystemType { get; set; }
        public string OriginalFilter { get; set; } = string.Empty;
        public bool StartsWithAnything { get; set; } = false;
        public bool EndsWithAnything { get; set; } = false;
        public FileSystemFilterItem(string original_text, FileSystemType file_system_type, bool starts_with_anything = false, bool ends_with_anything = false)
        {
            FileSystemType = file_system_type;
            OriginalFilter = original_text;
            StartsWithAnything = starts_with_anything;
            EndsWithAnything = ends_with_anything;
        }
    }
}
