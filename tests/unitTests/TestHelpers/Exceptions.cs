using System;
using System.Diagnostics;

namespace UnitTests.TestHelpers
{
    [DebuggerStepThrough]
    public class DummyException : Exception
    {
        public DummyException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DummyException()
        {
        }
    }

    [DebuggerStepThrough]
    public class SubClassedDummyException : DummyException
    {
        public SubClassedDummyException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public SubClassedDummyException()
        {
        }

    }
}