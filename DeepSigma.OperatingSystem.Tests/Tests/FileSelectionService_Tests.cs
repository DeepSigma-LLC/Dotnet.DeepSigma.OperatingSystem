
using Xunit;

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

        [Fact]
        public void MatchingFilterFound_ShouldReturnTrue2()
        {
            string test_directory = TestConfig.GetTestDataDirectoryPath();
            FileSystemSelectionService selected_file_serivce = new(test_directory, "AppIgnore.txt");

            bool found = selected_file_serivce.MatchingFilterFound("Example.txt");

            Assert.True(found);
        }


        [Fact]
        public void MatchingFilterFound_ShouldReturnTrue3()
        {
            string test_directory = TestConfig.GetTestDataDirectoryPath();
            FileSystemSelectionService selected_file_serivce = new(test_directory, "AppIgnore.txt");

            bool found = selected_file_serivce.MatchingFilterFound("Tester.json");
            Assert.True(found);

            bool found2 = selected_file_serivce.MatchingFilterFound("Tester.cs");
            Assert.True(found2);
        }



        [Fact]
        public void MatchingFilterFound_ShouldReturnTrue_Directroy()
        {
            string test_directory = TestConfig.GetTestDataDirectoryPath();
            FileSystemSelectionService selected_file_serivce = new(test_directory, "AppIgnore.txt");

            bool found = selected_file_serivce.MatchingFilterFound("Data");
            Assert.True(found);

            bool found2 = selected_file_serivce.MatchingFilterFound("ConfigTest");
            Assert.True(found2);

            bool found3 = selected_file_serivce.MatchingFilterFound("TimeSeries");
            Assert.True(found3);
        }
    }
}
