using System;

namespace ExceptionLayoutFormatter.ExceptionLayouts
{
    public interface IExceptionLayout<in TException> : IExceptionLayout
        where TException : Exception
    {
        string FormatException(IFormatter formatter, TException ex);
    }

    public interface IExceptionLayout
    {
    }
}