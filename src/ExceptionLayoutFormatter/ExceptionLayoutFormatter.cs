using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExceptionLayoutFormatter.ExceptionLayouts;

namespace ExceptionLayoutFormatter
{
    public class ExceptionLayoutFormatter
    {
        private readonly IDictionary<Type, Func<Type, IExceptionLayout>> _layoutFormatterInstances;

        public ExceptionLayoutFormatter()
        {
            _layoutFormatterInstances = new Dictionary<Type, Func<Type, IExceptionLayout>>();

            AddLayoutFormatter(new ExceptionLayout());
        }

        public void AddExceptionLayouts(Assembly assembly)
        {
            var exceptionLayouts = assembly.GetTypes()
                .Where(x => x.GetConstructor(Type.EmptyTypes) != null && x.GetInterfaces().Any(i => i == typeof(IExceptionLayout)))
                .ToList();

            foreach (var exceptionLayout in exceptionLayouts)
            {
                AddLayoutFormatter(exceptionLayout);
            }
        }

        public void AddLayoutFormatter(Type layoutType)
        {
            if (layoutType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExceptionLayout)))
                throw new ArgumentException("Should implement IExceptionLayout<T>", nameof(layoutType));
            if (!layoutType.IsClass)
                throw new ArgumentException("Should be an implementation of IExceptionLayout<T>", nameof(layoutType));
            if (layoutType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("Should have parameterless ctor", nameof(layoutType));

            Type exceptionTypeFromLayout;
            Func<Type, IExceptionLayout> exceptionLayoutResolver;
            
            if (layoutType.IsGenericType)
            {
                exceptionTypeFromLayout = layoutType.GetInterfaces().Single(x => x.IsGenericType).GetGenericArguments()[0].GetGenericTypeDefinition();

                exceptionLayoutResolver = exceptionType =>
                {
                    var exceptionPayloadType = exceptionType.GetGenericArguments().FirstOrDefault() ?? new { Dummy = "true" }.GetType();
                    var genericLayoutType = layoutType.GetGenericTypeDefinition().MakeGenericType(exceptionPayloadType);

                    return (IExceptionLayout)Activator.CreateInstance(genericLayoutType);
                };
            }
            else
            {
                exceptionTypeFromLayout = layoutType.GetInterfaces()[0].GetGenericArguments()[0];
                var exceptionLayout = (IExceptionLayout) Activator.CreateInstance(layoutType);

                exceptionLayoutResolver = exceptionType => exceptionLayout;
            }

            _layoutFormatterInstances[exceptionTypeFromLayout] = exceptionLayoutResolver;
        }

        public void AddLayoutFormatter<TException>(IExceptionLayout<TException> layout) where TException : Exception
        {
            _layoutFormatterInstances[typeof(TException)] = exceptionType => layout;
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

        internal List<IExceptionLayout> ExceptionLayouts => _layoutFormatterInstances.Values.Select(x => x.Invoke(typeof(Exception))).ToList();

        private IExceptionLayout<TException> FindMatchingLayoutFormatter<TException>() where TException : Exception
        {
            // Exact match
            var matchingFormatter = _layoutFormatterInstances
                .Where(x => x.Key == typeof(TException))
                .Select(x => x.Value)
                .FirstOrDefault();

            // Generic ExceptionLayout match
            if (typeof(TException).IsGenericType)
            {
                matchingFormatter = matchingFormatter ?? _layoutFormatterInstances
                    .Where(x => x.Key == typeof(TException).GetGenericTypeDefinition())
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }

            // BaseType match
            matchingFormatter = matchingFormatter ?? _layoutFormatterInstances
                .Where(x => x.Key != typeof(Exception) && x.Key.IsAssignableFrom(typeof(TException)))
                .Select(x => x.Value)
                .FirstOrDefault();

            // Default ExceptionFormatter
            var resolveFormatter = matchingFormatter ?? _layoutFormatterInstances[typeof(Exception)];

            return (IExceptionLayout<TException>)resolveFormatter(typeof(TException));
        }
    }
}