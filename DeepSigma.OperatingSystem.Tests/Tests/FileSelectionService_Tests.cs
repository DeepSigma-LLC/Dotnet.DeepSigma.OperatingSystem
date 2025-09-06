using Xunit;
using DeepSigma.OperatingSystem;

namespace DeepSigma.OperatingSystem.Tests.Tests
{
    public class FileSelectionService_Tests
    {
        [Fact]
        public void MatchingFilterFound_ShouldReturnTrue()
        {
            string test_directory = TestConfig.GetTestDataDirectoryPath();
            FileSystemSelectionService selected_file_serivce = new(test_directory, "AppIgnore.txt");

            bool found = selected_file_serivce.MatchingFilterFound("Config.json");

            Assert.True(found);
        }

        [Fact]
        public void MatchingFilterFound_ShouldReturnFalse()
        {
            string test_directory = TestConfig.GetTestDataDirectoryPath();
            FileSystemSelectionService selected_file_serivce = new(test_directory, "AppIgnore.txt");

            bool found = selected_file_serivce.MatchingFilterFound("Unknown.Fake");

            Assert.False(found);
        }
    }
}
