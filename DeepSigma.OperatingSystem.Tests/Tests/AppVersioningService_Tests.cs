using Xunit;
using DeepSigma.OperatingSystem;
using DeepSigma.OperatingSystem.Models;

namespace DeepSigma.OperatingSystem.Tests.Tests
{
    public class AppVersioningService_Tests
    {
        [Fact]
        public void GetLatestApplicationVersionFromDirectory_ShouldReturnNullForNonExistentDirectory()
        {
            string nonExistentDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            ApplicationVersion? result = AppVersioningService.GetLatestApplicationVersionFromDirectory(nonExistentDirectory);
            Assert.Null(result);
        }

        [Fact]
        public void GetLatestApplicationVersionFromDirectory_ShouldReturnValue()
        {
            ApplicationVersion? result = AppVersioningService.GetLatestApplicationVersionFromDirectory(GetTestDirectoryPath());
            Assert.NotNull(result);
        }

        [Fact]
        public void GetLatestApplicationVersionFromDirectory_ShouldMatchExpectedValue()
        {
            ApplicationVersion? actual = AppVersioningService.GetLatestApplicationVersionFromDirectory(GetTestDirectoryPath());
            ApplicationVersion expected = new(11, 11, 15, 110);
            Assert.Equal(expected, actual);
        }


        private string GetTestDirectoryPath()
        {
            string base_directory = AppDomain.CurrentDomain.BaseDirectory;

            for (int i = 0; i <= 3; i++)
            {
                DirectoryInfo? parent_directory = Directory.GetParent(base_directory);
                if (parent_directory is null) break;
                base_directory = parent_directory.FullName;
            }

            string full_directory_path = Path.Combine(base_directory, "Data", "TestDirectories_DoNotDelete");
            return full_directory_path;
        }
    }
}
