using Xunit;
using System.Diagnostics;
using DeepSigma.General.Monads;

namespace DeepSigma.OperatingSystem.Tests.Tests;

public class ProcessManager_Tests
{
    [Fact]
    public void GetAllActiveProcess_ShouldReturnProcesses()
    {
        var processes = ProcessManager.GetAllActiveProcess();
        Assert.NotNull(processes);
        Assert.True(processes.Length > 0);
    }

    [Fact]
    public void GetActiveProcessByName_ShouldReturnProcesses()
    {
        ResultMonad<Process[]> result = ProcessManager.GetActiveProcessByName("explorer");
        result.Switch(
            success =>
            {
                Assert.NotNull(success.Result);
                Assert.True(success.Result.Count() > 0);
            },
            error => Assert.Fail("Expected success but got error - " + error.Exception.Message)
            );
    }

    [Fact]
    public void GetActiveProcessByName_ShouldReturnErrorForEmptyName()
    {
        var result = ProcessManager.GetActiveProcessByName("");
        result.Switch(
            success => Assert.Fail("Expected error, but got success"),
            error => Assert.NotNull(error)
            );
    }

}
