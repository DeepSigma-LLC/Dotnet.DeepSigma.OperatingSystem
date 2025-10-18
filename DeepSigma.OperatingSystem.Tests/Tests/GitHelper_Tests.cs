using Xunit;

namespace DeepSigma.OperatingSystem.Tests.Tests;

public class GitHelper_Tests
{
    [Fact]
    public void DownloadGitRepository_InvalidUrl_ReturnsArgumentException()
    {
        string invalidUrl = "invalid-url";
        string targetDirectory = "some/target/directory";
        Exception? result = GitHelper.DownloadGitRepository(invalidUrl, targetDirectory);
        Assert.IsType<Exception>(result);
    }

    [Fact]
    public void DownloadGitRepository_ValidUrl_ShouldDownload()
    {
        // Arrange
        string validUrl = "https://github.com/DeepSigma-LLC/Dotnet.DeepSigma.UnitTest.git";
        string targetDirectory = TempFolderUtility.GetDownloadsPath() + @"\" + Guid.NewGuid().ToString();
        Exception? result = GitHelper.DownloadGitRepository(validUrl, targetDirectory);
        Assert.True(Directory.Exists(targetDirectory));
        Assert.Null(result);

        //Clean up
        FileSystem.FileSystemUtilities.ForceDeleteAllFilesAndDirectoriesRecursively(targetDirectory);
    }
}
