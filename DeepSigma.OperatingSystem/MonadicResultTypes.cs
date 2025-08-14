using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSigma.OperatingSystem
{
    public record Success<T>(T? Result);
    public record Error(ExceptionLog Exception);
    public record Errors(IEnumerable<ExceptionLog> Exceptions);
}
