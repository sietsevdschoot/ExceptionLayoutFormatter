using System;
using System.Diagnostics;

namespace UnitTests.TestHelpers
{
    [DebuggerStepThrough]
    public class CustomerException : Exception
    {
        public CustomerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CustomerException()
        {
        }
    }

    [DebuggerStepThrough]
    public class CustomerNotFoundException : CustomerException
    {
        public CustomerNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public CustomerNotFoundException()
        {
        }

    }
}