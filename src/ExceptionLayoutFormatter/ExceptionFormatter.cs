using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExceptionLayoutFormatter.ExceptionLayouts;

namespace ExceptionLayoutFormatter
{
    public class ExceptionFormatter : IExceptionFormatter
    {
        private readonly ExceptionExtractor _extractor;
        private readonly ExceptionLayoutFormatter _formatter;
        private readonly ExceptionLayoutsCollection _exceptionLayouts;

        public ExceptionFormatter(
            ExceptionLayoutsCollection exceptionLayouts,
            ExceptionExtractor extractor, 
            ExceptionLayoutFormatter formatter)
        {
            _exceptionLayouts = exceptionLayouts;
            _formatter = formatter;
            _extractor = extractor;
        }

        public static IExceptionFormatter Create(params Assembly[] assemblies)
        {
            var exceptionLayouts = new ExceptionLayoutsCollection()
                .AddExceptionLayout(new ExceptionLayout());

            foreach (var assembly in assemblies)
            {
                exceptionLayouts.AddExceptionLayouts(assembly);
            }
                
            return new ExceptionFormatter(
                exceptionLayouts: exceptionLayouts,
                extractor: new ExceptionExtractor(),
                formatter: new ExceptionLayoutFormatter(exceptionLayouts));
        }

        public ExceptionFormatter AddExceptionLayout<TException>(IExceptionLayout<TException> layout) where TException : Exception
        {
            _exceptionLayouts[typeof(TException)] = runtimeExceptionType => layout;
            
            return this;
        }

        public ExceptionFormatter AddExceptionLayout(Type layoutType)
        {
            _exceptionLayouts.AddExceptionLayout(layoutType);
            
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

        internal List<IExceptionLayout> ExceptionLayouts => _exceptionLayouts.ToList();
    }

    public interface IExceptionFormatter
    {
        ExceptionFormatter AddExceptionLayout<TException>(IExceptionLayout<TException> layout) where TException : Exception;
        ExceptionFormatter AddExceptionLayout(Type layoutType);
        string FormatException(Exception exception);
    }
}