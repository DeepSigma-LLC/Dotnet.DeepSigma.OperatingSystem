

namespace DeepSigma.OperatingSystem;

internal class FileSystemFilterAccess
{
    private string DirectoryPath { get; set; }
    private string FileName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemFilterAccess"/> class.
    /// </summary>
    /// <param name="directory_path">Directory path of the ignore file.</param>
    /// <param name="file_name">File name of the ignore file.</param>
    internal FileSystemFilterAccess(string directory_path, string file_name)
    {
        this.DirectoryPath = directory_path;
        this.FileName = file_name;
    }

    /// <summary>
    /// Gets ignore filters from designated file if it exists.
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<string> GetIgnoreFilters()
    {
        string FullPath = Path.Combine(DirectoryPath, FileName);
        if (!File.Exists(FullPath)) { yield break; }

        string[] fileLines = File.ReadAllLines(FullPath);
        foreach (string line in fileLines)
        {
            string[] words = line.Split(" ");
            foreach (string word in words)
            {
                if (IsComment(word))
                {
                    break;
                }
                yield return word.Trim();
            }
        }
    }

    /// <summary>
    /// Determines if text is a comment as I define it.
    /// Comments start with # and last the entire line.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static bool IsComment(string text)
    {
        return text.Trim().StartsWith("#");
    }
}
