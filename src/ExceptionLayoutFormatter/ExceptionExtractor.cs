using System;
using System.Collections.Generic;
using System.Linq;

namespace ExceptionLayoutFormatter
{
    public class ExceptionExtractor
    {
        public List<Exception> ExtractAllExceptions(Exception exception)
        {
            return GetAllExceptions(exception).Reverse().ToList();
        }

        private IEnumerable<Exception> GetAllExceptions(Exception exception)
        {
            if (exception != null)
            {
                yield return exception;

                foreach (var inner in GetInnerExceptions(exception).SelectMany(GetAllExceptions))
                {
                    yield return inner;
                }
            }
        }

        private IEnumerable<Exception> GetInnerExceptions(Exception ex)
        {
            IEnumerable<Exception> innerExceptions;

            if (ex is AggregateException aggregateException)
            {
                innerExceptions = aggregateException.InnerExceptions.ToList();
            }
            else
            {
                innerExceptions = ex.GetType().GetProperties()
                    .Where(x => typeof(IEnumerable<Exception>).IsAssignableFrom(x.PropertyType))
                    .SelectMany(x => (IEnumerable<Exception>)x.GetValue(ex))
                    .Concat(new List<Exception> {ex.InnerException})
                    .Distinct();
            }

            return innerExceptions;
        }
    }
}