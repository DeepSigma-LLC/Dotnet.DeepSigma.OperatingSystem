using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSigma.OperatingSystem
{
    public static class ProcessManager
    {
        /// <summary>
        /// Get all processes running on the local machine
        /// </summary>
        /// <returns></returns>
        public static Process[] GetAllActiveProcess()
        {
            return Process.GetProcesses();
        }

        public static ResultMonad<Process[]> GetActiveProcessByName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                var exception = new ArgumentException("Process name cannot be null or empty.", nameof(processName));
                return new Error(new ExceptionLog(exception, "Process name cannot be null or empty."));
            }
            return new Success<Process[]>(Process.GetProcessesByName(processName));
        }


        /// <summary>
        /// Attempts to kill all processes with the specified name. If successful, returns the count of processes killed.
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        public static ResultMonadMultiError<int> KillProcessByName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return new Errors( [new ExceptionLog( new ArgumentException("Process name cannot be null or empty.", nameof(processName)))]);
            }

            List<ExceptionLog> error_log = [];
            int count = 0;
            Process[]? processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                try
                {
                    process.Kill();
                    count++;
                }
                catch (Exception ex)
                {
                    error_log.Add(new ExceptionLog(ex, $"Failed to kill process {processName}."));
                }
            }

            if (error_log.Count >= 1)
            {
                return new Errors(error_log.ToArray());
            }
            return new Success<int>(count);
        }

    }
}
