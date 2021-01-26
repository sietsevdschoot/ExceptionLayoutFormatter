using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExceptionLayoutFormatter.ExceptionLayouts;

namespace ExceptionLayoutFormatter
{
    public class ExceptionFormatter
    {
        private readonly ExceptionExtractor _extractor;
        private readonly ExceptionLayoutFormatter _formatter;

        public ExceptionFormatter(ExceptionLayoutFormatter formatter, ExceptionExtractor extractor)
        {
            _formatter = formatter;
            _extractor = extractor;
        }

        public static ExceptionFormatter Create(params Assembly[] assemblies)
        {
            var layoutFormatter = new ExceptionLayoutFormatter();

            foreach (var assembly in assemblies)
            {
                layoutFormatter.AddExceptionLayouts(assembly);
            }

            return new ExceptionFormatter(layoutFormatter, new ExceptionExtractor());
        }

        public ExceptionFormatter AddExceptionLayout<TException>(IExceptionLayout<TException> layout) where TException : Exception
        {
            _formatter.AddLayoutFormatter(layout);
            
            return this;
        }

        public string FormatException(Exception exception)
        {
            var allExceptions = _extractor.ExtractAllExceptions(exception);

            var formattedExceptions = new List<string>();

            foreach (dynamic ex in allExceptions)
            {
                formattedExceptions.Add(_formatter.CreateFormattedExceptionString(ex));
            }

            return string.Join(Environment.NewLine, formattedExceptions);
        }

        internal List<IExceptionLayout> ExceptionLayouts => _formatter.ExceptionLayouts;
    }
}