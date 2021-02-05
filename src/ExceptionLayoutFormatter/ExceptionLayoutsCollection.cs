using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using ExceptionLayoutFormatter.ExceptionLayouts;
using ExceptionLayoutFormatter.Extensions;

namespace ExceptionLayoutFormatter
{
    [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
    public class ExceptionLayoutsCollection : IEnumerable<IExceptionLayout>
    {
        /// <summary>
        /// <code> IDictionary<ExceptionTypeFromLayout, Func<RuntimeExceptionType, IExceptionType>> </code>
        /// </summary>
        private readonly IDictionary<Type, Func<Type, IExceptionLayout>> _exceptionLayoutResolvers;

        public ExceptionLayoutsCollection()
        {
            _exceptionLayoutResolvers = new Dictionary<Type, Func<Type, IExceptionLayout>>();
        }

        public ExceptionLayoutsCollection AddExceptionLayout<TException>(IExceptionLayout<TException> layout) where TException : Exception
        {
            _exceptionLayoutResolvers[typeof(TException)] = runtimeExceptionType => layout;

            return this;
        }

        public ExceptionLayoutsCollection AddExceptionLayouts(Assembly assembly)
        {
            var exceptionLayoutTypes = assembly.ExportedTypes
                .Where(x => x.GetConstructor(Type.EmptyTypes) != null && x.GetInterfaces().Any(i => i == typeof(IExceptionLayout)))
                .ToList();

            foreach (var exceptionLayoutType in exceptionLayoutTypes)
            {
                AddExceptionLayout(exceptionLayoutType);
            }

            return this;
        }

        public void AddExceptionLayout(Type layoutType)
        {
            if (layoutType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IExceptionLayout)))
                throw new ArgumentException("Should implement IExceptionLayout<T>", nameof(layoutType));
            if (!layoutType.IsClass || layoutType.IsAbstract)
                throw new ArgumentException("Should be an implementation of IExceptionLayout<T>", nameof(layoutType));
            if (layoutType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("Should have parameterless ctor", nameof(layoutType));

            if (layoutType.IsGenericType) 
            {
                var genericExceptionTypeFromLayout = layoutType.GetInterfaces()
                    .Single(x => x.IsGenericType).GetGenericArguments()[0].GetGenericTypeDefinition();

                _exceptionLayoutResolvers[genericExceptionTypeFromLayout] = runtimeExceptionType =>
                {
                    var exceptionPayloadType = runtimeExceptionType.GetGenericArguments().FirstOrDefault() ?? new { Dummy = "true" }.GetType();
                    var genericLayoutTypeForCurrentException = layoutType.GetGenericTypeDefinition().MakeGenericType(exceptionPayloadType);

                    return (IExceptionLayout)Activator.CreateInstance(genericLayoutTypeForCurrentException);
                };
            }
            else
            {
                var exceptionTypeFromLayout = layoutType.GetInterfaces()[0].GetGenericArguments()[0];
                var exceptionLayout = (IExceptionLayout)Activator.CreateInstance(layoutType);

                _exceptionLayoutResolvers[exceptionTypeFromLayout] = runtimeExceptionType => exceptionLayout;
            }
        }

        public IExceptionLayout<TException> FindMatchingExceptionLayout<TException>() where TException : Exception
        {
            // Exact match
            var matchingLayoutResolver = _exceptionLayoutResolvers.GetValueOrDefault(typeof(TException));

            // Generic match
            if (typeof(TException).IsGenericType)
            {
                matchingLayoutResolver = matchingLayoutResolver ?? _exceptionLayoutResolvers
                    .GetValueOrDefault(typeof(TException).GetGenericTypeDefinition());
            }

            // BaseType match
            matchingLayoutResolver = matchingLayoutResolver ?? _exceptionLayoutResolvers
                    .Where(x => x.Key != typeof(Exception) && x.Key.IsAssignableFrom(typeof(TException)))
                    .Select(x => x.Value)
                    .FirstOrDefault();

            // Default ExceptionLayout
            var layoutResolver = matchingLayoutResolver ?? _exceptionLayoutResolvers[typeof(Exception)];

            return (IExceptionLayout<TException>)layoutResolver(typeof(TException));
        }

        public IEnumerator<IExceptionLayout> GetEnumerator()
        {
            return _exceptionLayoutResolvers.Values.Select(x => x.Invoke(typeof(Exception))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Func<Type, IExceptionLayout> this[Type layoutExceptionType]
        {
            get => _exceptionLayoutResolvers[layoutExceptionType];
            set => _exceptionLayoutResolvers[layoutExceptionType] = value;
        }
    }
}