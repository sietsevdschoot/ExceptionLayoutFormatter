using System;
using System.Diagnostics;
using ExceptionLayoutFormatter;
using ExceptionLayoutFormatter.ExceptionLayouts;

namespace UnitTests.TestHelpers
{
    [DebuggerStepThrough]
    public class DefaultExceptionLayout : IExceptionLayout<Exception>
    {
        public string FormatException(IFormatter formatter, Exception ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
    public class CustomerExceptionLayout : IExceptionLayout<CustomerException>
    {
        public string FormatException(IFormatter formatter, CustomerException ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
    public class CustomerNotFoundExceptionLayout : IExceptionLayout<CustomerNotFoundException>
    {
        public string FormatException(IFormatter formatter, CustomerNotFoundException ex)
        {
            return this.GetType().Name;
        }
    }
}