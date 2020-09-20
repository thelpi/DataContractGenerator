using System;

namespace DataContractGenerator
{
    /// <summary>
    /// Logger interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">Message.</param>
        void Log(string message);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="exception">Exception.</param>
        void Log(Exception exception);
    }
}
