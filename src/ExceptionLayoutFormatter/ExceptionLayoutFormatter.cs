﻿using System;

namespace ExceptionLayoutFormatter
{
    public class ExceptionLayoutFormatter
    {
        private readonly ExceptionLayoutsCollection _exceptionLayouts;

        public ExceptionLayoutFormatter(ExceptionLayoutsCollection exceptionLayouts)
        {
            _exceptionLayouts = exceptionLayouts;
        }

        public string CreateFormattedExceptionString<TException>(TException exception)
            where TException : Exception
        {
            var exceptionLayout = _exceptionLayouts.FindMatchingExceptionLayout<TException>();

            string formattedException;

            try
            {
                formattedException = exceptionLayout.FormatException(new ExceptionFormattingUtil(), exception);
            }
            catch (Exception ex)
            {
                var defaultLayout = _exceptionLayouts.FindMatchingExceptionLayout<Exception>();
                var formattingUtil = new ExceptionFormattingUtil();

                formattedException = string.Format(
                    "Exception in '{0}'\n{1}\nFalling back to default ExceptionLayout.\n\nOriginal Exception:\n{2}",
                    exceptionLayout.GetType().FullName, 
                    defaultLayout.FormatException(formattingUtil, ex), 
                    defaultLayout.FormatException(formattingUtil, exception));
            }

            return formattedException;
        }
    }
}