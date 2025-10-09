using DeepSigma.General.Monads;
namespace DeepSigma.OperatingSystem;

/// <summary>
/// A class to execute Python scripts and manage Python environments.
/// </summary>
public static class PythonExecutor
{
    /// <summary>
    /// Executes a Python script and returns the output.
    /// </summary>
    /// <param name="script_path">The path to the Python script.</param>
    /// <param name="script_args">Optional arguments to pass to the script.</param>
    /// <param name="python_exe_file_path">Optional python.exe path. Enables users to select a specific virutal enviornment executor.</param>
    /// <returns>The output of the script execution.</returns>
    public static ResultMonad<string> ExecuteScript(string script_path, string? script_args = null, string? python_exe_file_path = null)
    {
        if(python_exe_file_path is null)
        {
            return Terminal.RunCommand("python", $"\"{script_path}\" {script_args}");
        }
        return Terminal.RunCommand(python_exe_file_path, $"\"{script_path}\" {script_args}");
    }

    /// <summary>
    /// Gets Python virtual environments available on the system.
    /// </summary>
    /// <returns></returns>
    public static ResultMonad<string[]> GetPythonVirtualEnvironments()
    {
        string py_launcher_command = "py";
        ResultMonad<string> result =  Terminal.RunCommand(py_launcher_command, "--list");

        ResultMonad<string[]> final_results = result.Match<ResultMonad<string[]>>(
              success => new Success<string[]>(success.Result?.Split(" ")),
              error => new Error(error.Exception)
        );

        return final_results;
    }

    /// <summary>
    /// Gets install locations of Python on the system.
    /// </summary>
    /// <returns></returns>
    public static ResultMonad<string[]> GetPythonInstallLocations()
    {
        ResultMonad<string> result = Terminal.RunCommand("where", "py");

        ResultMonad<string[]> final_results = result.Match<ResultMonad<string[]>>(
            success => new Success<string[]>(success.Result?.Split(";")),
            error => new Error(error.Exception)
        );

        return final_results;
    }

    /// <summary>
    /// Gets install locations of Python on the system.
    /// </summary>
    /// <returns></returns>
    public static bool IsPythonInstalled()
    {
        return Terminal.IsProgramInstalled("python") || Terminal.IsProgramInstalled("py");
    }
}
