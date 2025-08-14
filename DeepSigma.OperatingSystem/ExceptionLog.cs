using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSigma.OperatingSystem
{
    public class ExceptionLog(Exception exception, string? friendly_message = null)
    {
        public string? FriendlyMessage { get; set; } = friendly_message;
        public Exception Exception { get; set; } = exception;
    }
}
