using System;
using ModestTree;

namespace ModestTree
{
    [System.Diagnostics.DebuggerStepThrough]
    public class YaslException : Exception
    {
        public YaslException(string message)
            : base(message)
        {
        }

        public YaslException(
            string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
