using System;
using System.Collections.Generic;
using DataContractGenerator;

namespace DataContractGeneratorUnitTests
{
    public class TestLogger : ILogger
    {
        private readonly List<string> _messages = new List<string>();

        public void Log(string message)
        {
            _messages.Add(message);
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Log(Exception exception)
        {
            _messages.Add(exception.Message);
            System.Diagnostics.Debug.WriteLine(exception.Message);
            System.Diagnostics.Debug.WriteLine(exception.StackTrace);
        }

        public void Clear()
        {
            _messages.Clear();
        }

        public int ErrorsCount { get { return _messages.Count; } }
    }
}
