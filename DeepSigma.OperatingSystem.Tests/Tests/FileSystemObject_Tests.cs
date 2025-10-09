using Xunit;
using DeepSigma.OperatingSystem.Enums;

namespace DeepSigma.OperatingSystem.Tests.Tests;

public class FileSystemObject_Tests
{
    [Fact]
    public void FileSystemObject_ShouldReturnDirectories()
    {
        string test_directory = TestConfig.GetTestVersionDirectoryPath();
        FileSystemObject fso = new(test_directory, 10);
        FileSystemObject[] directories = fso.Directories;
        Assert.NotNull(directories);
        Assert.True(directories.Length > 0);
    }

    [Fact]
    public void FileSystemObject_ShouldReturnFiles()
    {
        string test_directory = TestConfig.GetTestVersionDirectoryPath();
        FileSystemObject fso = new(test_directory, 10);
        FileSystemObject[] files = fso.Files;
        Assert.NotNull(files);
        Assert.True(files.Length > 0);
    }



    [Fact]
    public void FileSystemObject_ShouldBeDirectory()
    {
        string test_directory = TestConfig.GetTestDataDirectoryPath();
        FileSystemObject fso = new(test_directory, 1);
        Assert.True(fso.ObjectSystemType == FileSystemType.Directory);
    }

    [Fact]
    public void FileSystemObject_ShouldBeFile()
    {
        string test_directory = TestConfig.GetTestVersionDirectoryPath();
        FileSystemObject fso = new(test_directory, 2);
        FileSystemObject[] files = fso.Files;
        Assert.True(files[0].ObjectSystemType == FileSystemType.File);
    }

    [Fact]
    public void FileSystemObject_FilePropertiesShouldBeSet()
    {
        string test_directory = TestConfig.GetTestVersionDirectoryPath();
        FileSystemObject fso = new(test_directory, 2);
        FileSystemObject[] files = fso.Files;
        FileSystemObject first_file = files[0];
        Assert.NotNull(first_file.FileProperties.FileName);
        Assert.NotNull(first_file.FileProperties.FileNameWithoutExtension);
        Assert.NotNull(first_file.FileProperties.FileExtension);
        Assert.NotNull(first_file.FileProperties.CreationTimeUtc);
        Assert.NotNull(first_file.FileProperties.LastWriteTimeUtc);
        Assert.NotNull(first_file.FileProperties.LastAccessTimeUtc);
    }

    [Fact]
    public void FileSystemObject_FileNameShouldNotBeAPath()
    {
        string test_directory = TestConfig.GetTestVersionDirectoryPath();
        FileSystemObject fso = new(test_directory, 2);
        FileSystemObject[] files = fso.Files;
        FileSystemObject first_file = files[0];
        bool result = File.Exists(first_file.FileProperties.FileName);
        
        Assert.False(result);
    }

    [Fact]
    public void FileSystemObject_DirectoryNameShouldNotBeAPath()
    {
        string test_directory = TestConfig.GetTestVersionDirectoryPath();
        FileSystemObject fso = new(test_directory, 2);
        bool result = Directory.Exists(fso.DirectoryName);

        Assert.False(result);
    }
}
