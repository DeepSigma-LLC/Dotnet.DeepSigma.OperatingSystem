using Xunit;
using System.Diagnostics;
using DeepSigma.Core.Monads;

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
        Assert.True(result is SuccessResult<Process[]>);
        if (result is SuccessResult<Process[]> success)
        {
            Assert.NotNull(success.Result);
            Assert.True(success.Result.Length > 0);
        }
    }

    [Fact]
    public void GetActiveProcessByName_ShouldReturnErrorForEmptyName()
    {
        var result = ProcessManager.GetActiveProcessByName("");
        Assert.True(result is ErrorResult);
        if (result is ErrorResult error)
            Assert.IsType<ArgumentException>(error.Exception);
    }

}
