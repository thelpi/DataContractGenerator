using System;

namespace DataContractGenerator
{
    /// <summary>
    /// Default implementation of <see cref="ILogger"/>
    /// </summary>
    /// <seealso cref="ILogger"/>
    public class DefaultLogger : ILogger
    {
        /// <inheritdoc />
        public void Log(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                System.Diagnostics.Debug.WriteLine(message);
            }
        }

        /// <inheritdoc />
        public void Log(Exception exception)
        {
            if (!string.IsNullOrWhiteSpace(exception?.Message))
            {
                System.Diagnostics.Debug.WriteLine(exception.Message);
            }
        }
    }
}
