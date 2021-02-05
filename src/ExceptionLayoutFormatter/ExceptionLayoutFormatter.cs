using System;

namespace ExceptionLayoutFormatter
{
    public class ExceptionLayoutFormatter
    {
        private readonly ExceptionLayoutsCollection _exceptionLayouts;

        public ExceptionLayoutFormatter(ExceptionLayoutsCollection exceptionLayouts)
        {
            _exceptionLayouts = exceptionLayouts;
        }

        public string CreateFormattedExceptionString<TException>(TException ex)
            where TException : Exception
        {
            var exceptionLayout = _exceptionLayouts.FindMatchingExceptionLayout<TException>();

            string formattedException;

            try
            {
                formattedException = exceptionLayout.FormatException(new ExceptionFormattingUtil(),  ex);
            }
            catch (Exception e)
            {
                var defaultExceptionLayout = _exceptionLayouts.FindMatchingExceptionLayout<Exception>();

                formattedException = string.Format(
                    "Exception in '{0}'\n{1}\nFalling back to default ExceptionLayout.\n\nOriginal Exception:\n{2}",
                    exceptionLayout.GetType().FullName, 
                    defaultExceptionLayout.FormatException(new ExceptionFormattingUtil(), e), 
                    defaultExceptionLayout.FormatException(new ExceptionFormattingUtil(), ex));
            }

            return formattedException;
        }
    }
}