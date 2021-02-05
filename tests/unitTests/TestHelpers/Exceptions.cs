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

    [DebuggerStepThrough]
    public class OrderException : Exception
    {
        public OrderException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public OrderException(string message) : base(message)
        {
        }

        public OrderException()
        {
        }
    }
}