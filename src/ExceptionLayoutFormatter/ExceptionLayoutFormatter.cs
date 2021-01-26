using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExceptionLayoutFormatter.ExceptionLayouts;

namespace ExceptionLayoutFormatter
{
    public class ExceptionLayoutFormatter
    {
        private readonly IDictionary<Type, IExceptionLayout> _layoutFormatterInstances;

        public ExceptionLayoutFormatter()
        {
            _layoutFormatterInstances = new Dictionary<Type, IExceptionLayout>();

            AddLayoutFormatter(new ExceptionLayout());
        }

        public void AddLayoutFormatter<TException>(IExceptionLayout<TException> layout) where TException : Exception
        {
            _layoutFormatterInstances[typeof(TException)] = layout;
        }

        public void AddExceptionLayouts(Assembly assembly)
        {
            var exceptionLayouts = assembly.GetTypes().Where(x =>
                x.GetConstructor(Type.EmptyTypes) != null && !x.ContainsGenericParameters &&
                x.GetInterfaces().Any(i => i == typeof(IExceptionLayout)));

            var layoutFormatters = exceptionLayouts.Select(exceptionLayout => new
            {
                LayoutFormatter = (IExceptionLayout)Activator.CreateInstance(exceptionLayout),

                ExceptionType = exceptionLayout.GetInterfaces()
                    .Single(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExceptionLayout<>))
                    .GetGenericArguments()[0]
            });

            foreach (var layoutFormatter in layoutFormatters)
            {
                _layoutFormatterInstances[layoutFormatter.ExceptionType] = layoutFormatter.LayoutFormatter;
            }
        }

        public string CreateFormattedExceptionString<TException>(TException ex)
            where TException : Exception
        {
            var formatter = FindMatchingLayoutFormatter<TException>();
            string formattedException;

            try
            {
                formattedException = formatter.FormatException(new ExceptionFormattingUtil(),  ex);
            }
            catch (Exception e)
            {
                var defaultFormatter = FindMatchingLayoutFormatter<Exception>();

                formattedException = string.Format(
                    "Exception in '{0}'\nMessage: {1}\n Falling back to default formatter.\n\n{2}",
                    formatter.GetType().FullName, 
                    e.Message, 
                    defaultFormatter.FormatException(new ExceptionFormattingUtil(), ex));
            }

            return formattedException;
        }

        internal List<IExceptionLayout> ExceptionLayouts => _layoutFormatterInstances.Values.ToList();

        private IExceptionLayout<TException> FindMatchingLayoutFormatter<TException>() where TException : Exception
        {
            // Exact match
            var matchingFormatter = _layoutFormatterInstances
                .Where(x => x.Key == typeof(TException))
                .Select(x => x.Value)
                .FirstOrDefault();

            // BaseType match
            matchingFormatter = matchingFormatter ?? _layoutFormatterInstances
                .Where(x => x.Key != typeof(Exception) && x.Key.IsAssignableFrom(typeof(TException)))
                .Select(x => x.Value)
                .FirstOrDefault();

            // Default ExceptionFormatter
            var formatter = matchingFormatter ?? _layoutFormatterInstances[typeof(Exception)];

            return (IExceptionLayout<TException>)formatter;
        }
    }
}