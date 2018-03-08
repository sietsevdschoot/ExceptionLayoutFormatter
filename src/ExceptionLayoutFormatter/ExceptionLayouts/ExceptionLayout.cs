using System;

namespace ExceptionLayoutFormatter.ExceptionLayouts
{
    public class ExceptionLayout : IExceptionLayout<Exception>
    {
        public string FormatException(IFormatter formatter, Exception ex)
        {
            return formatter.GetFormattedException(ex);
        }
    }
}