using OneOf;
using OneOf.Types;
using System;
using System.Collections.Generic;

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
    public class ResultMonad<T> : OneOfBase<Success<T>, Error>
    {
        public ResultMonad(OneOf<Success<T>, Error> input) : base(input) { }

        public static implicit operator ResultMonad<T>(Success<T> success) => new(success);
        public static implicit operator ResultMonad<T>(Error error) => new(error);
    }
}
