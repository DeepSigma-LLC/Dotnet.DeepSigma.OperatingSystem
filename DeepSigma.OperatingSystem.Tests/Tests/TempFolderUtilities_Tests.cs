using Xunit;

namespace DeepSigma.OperatingSystem.Tests.Tests
{
    public class TempFolderUtilities_Tests
    {
        [Fact]
        public void CreateTempDirectoryInDownloads_ShouldCreateTempDirectory()
        {
            DirectoryInfo directory = TempFolderUtility.CreateTempDirectoryInDownloads();
            bool directory_exists = directory.Exists;

            Assert.True(directory_exists);

            Directory.Delete(directory.FullName);
        }


        [Fact]
        public void GetDownloadPath_ShouldGetDirectoryPath()
        {
            string? full_directory_path = TempFolderUtility.GetDownloadsPath();

            Assert.True(full_directory_path is not null);
        }


        [Fact]
        public void GetDownloadPath_ShouldGetCorrectDirectoryPath()
        {
            string? actual = TempFolderUtility.GetDownloadsPath();
            string expected = Path.Combine(@"C:\Users", Environment.UserName, "Downloads");
            Assert.Equal(expected, actual);
        }
    }
}
