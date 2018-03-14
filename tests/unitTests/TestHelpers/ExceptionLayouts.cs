using System;
using System.Diagnostics;
using ExceptionLayoutFormatter;
using ExceptionLayoutFormatter.ExceptionLayouts;

namespace UnitTests.TestHelpers
{
    [DebuggerStepThrough]
    public class EmptyExceptionLayout : IExceptionLayout<Exception>
    {
        public string FormatException(IFormatter formatter, Exception ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
    public class DummyExceptionLayout : IExceptionLayout<DummyException>
    {
        public string FormatException(IFormatter formatter, DummyException ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
    public class SubClassedDummyExceptionLayout : IExceptionLayout<SubClassedDummyException>
    {
        public string FormatException(IFormatter formatter, SubClassedDummyException ex)
        {
            return this.GetType().Name;
        }
    }

    [DebuggerStepThrough]
    public class GenericTestExceptionLayout<TException> : IExceptionLayout<TException> where TException : Exception
    {
        public string FormatException(IFormatter formatter, TException ex)
        {
            return typeof(TException).Name;
        }
    }
}