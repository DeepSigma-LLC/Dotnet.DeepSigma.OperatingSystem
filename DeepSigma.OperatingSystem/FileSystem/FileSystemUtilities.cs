
namespace DeepSigma.OperatingSystem.FileSystem;


public static class FileSystemUtilities
{

    public static void ForceDeleteAllFilesAndDirectoriesRecursively(string repoPath)
    {
        if (Directory.Exists(repoPath))
        {
            // Remove read-only attributes
            foreach (string file in Directory.GetFiles(repoPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }

            Directory.Delete(repoPath, recursive: true);
        }
    }
}
