using DeepSigma.General.Monads;

namespace DeepSigma.OperatingSystem
{
    public static class PythonExecutor
    {
        /// <summary>
        /// Executes a Python script and returns the output.
        /// </summary>
        /// <param name="script_path">The path to the Python script.</param>
        /// <param name="script_args">Optional arguments to pass to the script.</param>
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
        public static ResultMonad<string> GetPythonVirtualEnvironments()
        {
            string py_launcher_command = "py";
            return Terminal.RunCommand(py_launcher_command, "--list");
        }

        /// <summary>
        /// Gets install locations of Python on the system.
        /// </summary>
        /// <returns></returns>
        public static ResultMonad<string> GetPythonInstallLocations()
        {
            return Terminal.RunCommand("where", "python");
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
}
