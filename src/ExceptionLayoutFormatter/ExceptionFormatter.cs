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

        public static ExceptionFormatter Create(params Assembly[] assemblies)
        {
            var layoutFormatter = new ExceptionLayoutFormatter();

            assemblies.ToList().ForEach(layoutFormatter.AddExceptionLayouts);

            return new ExceptionFormatter(layoutFormatter, new ExceptionExtractor());
        }

        internal static ExceptionFormatter Create(IDictionary<Type, IExceptionLayout> formatters)
        {
            var extractor = new ExceptionExtractor();
            var layoutFormatter = new ExceptionLayoutFormatter();

            foreach (dynamic entry in formatters)
            {
                layoutFormatter.AddLayoutFormatter(entry.Value);
            }

            var formatter = new ExceptionFormatter(layoutFormatter, extractor);

            return formatter;
        }

        public ExceptionFormatter(ExceptionLayoutFormatter formatter, ExceptionExtractor extractor)
        {
            _formatter = formatter;
            _extractor = extractor;
        }

        public string FormatException(Exception exception)
        {
            var allExceptions = _extractor.ExtractAllExceptions(exception);
            var formattedExceptionMessages = FormatAllExceptionsToErrorMessage(allExceptions);

            return string.Join(Environment.NewLine, formattedExceptionMessages);
        }

        private List<string> FormatAllExceptionsToErrorMessage(List<Exception> allExceptions)
        {
            var formattedExceptions = new List<string>();

            foreach (dynamic exception in allExceptions)
            {
                formattedExceptions.Add(_formatter.CreateFormattedExceptionString(exception));
            }

            return formattedExceptions;
        }
    }
}