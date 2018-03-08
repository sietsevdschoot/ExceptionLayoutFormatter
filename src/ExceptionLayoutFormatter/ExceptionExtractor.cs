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
            var exceptionsWithInnerExceptions = new List<string>
            {
                "System.Reflection.ReflectionTypeLoadException",
                "System.Net.Mail.SmtpFailedRecipientsException",
                "System.ComponentModel.Composition.ChangeRejectedException",
                "System.ComponentModel.Composition.CompositionException",
            };

            IEnumerable<Exception> innerExceptions;

            if (exceptionsWithInnerExceptions.Contains(ex.GetType().FullName))
            {
                innerExceptions = (IEnumerable<Exception>)ex.GetType().GetProperties()
                      .Single(x => typeof(IEnumerable<Exception>).IsAssignableFrom(x.PropertyType))
                      .GetValue(ex);
            }
            else
            {
                innerExceptions = ex is AggregateException aggregateException
                    ? aggregateException.InnerExceptions.ToList()
                    : new List<Exception> { ex.InnerException };
            }

            return innerExceptions;
        }
    }
}