using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepSigma.OperatingSystem
{
    /// <summary>
    /// Generic <see cref="ResultMonad{T}"/> type that encapsulates the result of an operation.
    ///
    /// Usage example:
    /// <code>
    ///   var result = new Success&lt;string&gt;("ok");
    ///
    ///   // Match on Monad&lt;T&gt; to get a single value.
    ///   string msg = result.Match(
    ///       success   => $"Success: {success.Result}",
    ///       errors    => $"Errors: {string.Join(", ", errors.Exceptions.Select(e => e.FriendlyMessage))}");
    ///
    ///   // Switching on Monad&lt;T&gt; to deal with side effects.
    ///   result.Switch(
    ///       success => Console.WriteLine($"Success: {success.Result}"),
    ///       errors  => Console.WriteLine($"Errors: {string.Join(", ", errors.Exceptions.Select(e => e.FriendlyMessage))}"));
    /// </code>
    /// </summary>
    /// <typeparam name="T">The wrapped success value type.</typeparam>
    public class ResultMonadMultiError<T> : OneOfBase<Success<T>, Errors>
    {
        public ResultMonadMultiError(OneOf<Success<T>, Errors> input) : base(input) { }

        public static implicit operator ResultMonadMultiError<T>(Success<T> success) => new(success);
        public static implicit operator ResultMonadMultiError<T>(Errors errors) => new(errors);
    }
}
