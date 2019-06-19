using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace PerformanceTests
{
    [ClrJob(baseline: true), CoreJob, MonoJob, CoreRtJob]
    [RPlotExporter, RankColumn]
    [IterationCount(10)]
    public class ExceptionExtractionPerformanceTests
    {
        private List<Exception> data;

        [GlobalSetup]
        public void Setup()
        {
            data = new List<Exception>
            {
                new SmtpFailedRecipientsException("failed", new[]
                {
                    new SmtpFailedRecipientException("Inner 1"),
                    new SmtpFailedRecipientException("Inner 2"),
                    new SmtpFailedRecipientException("Inner 3"),
                }),
                new ReflectionTypeLoadException(new Type[0], new[]
                {
                    new Exception("Inner 1"),
                    new Exception("Inner 2"),
                    new Exception("Inner 3"),
                }),
                new AggregateException("Aggregate", new List<Exception>
                {
                    new Exception("Inner 1"),
                    new Exception("Inner 2"),
                    new Exception("Inner 3"),
                })
            };
        }

        [Benchmark]
        public List<Exception> DynamicExceptions()
        {
            List<Exception> innerExceptions = null;

            foreach (var ex in data)
            {
                if (ex is AggregateException aggregateException)
                {
                    innerExceptions = aggregateException.InnerExceptions.ToList();
                }
                else
                {
                    innerExceptions = ex.GetType().GetProperties()
                        .Where(x => typeof(IEnumerable<Exception>).IsAssignableFrom(x.PropertyType))
                        .SelectMany(x => (IEnumerable<Exception>)x.GetValue(ex))
                        .Concat(new List<Exception> { ex.InnerException })
                        .Distinct()
                        .ToList();
                }
            }

            return innerExceptions;
        }

        [Benchmark]
        public List<Exception> PresetExceptions()
        {
            List<Exception> innerExceptions = null;

            foreach (var ex in data)
            {
                var exceptionsWithInnerExceptions = new List<string>
                {
                    "System.Reflection.ReflectionTypeLoadException",
                    "System.Net.Mail.SmtpFailedRecipientsException",
                    "System.ComponentModel.Composition.ChangeRejectedException",
                    "System.ComponentModel.Composition.CompositionException",
                };

                if (exceptionsWithInnerExceptions.Contains(ex.GetType().FullName))
                {
                    innerExceptions = (List<Exception>)ex.GetType().GetProperties()
                        .Single(x => typeof(IEnumerable<Exception>).IsAssignableFrom(x.PropertyType))
                        .GetValue(ex);
                }
                else
                {
                    innerExceptions = ex is AggregateException aggregateException
                        ? aggregateException.InnerExceptions.ToList()
                        : new List<Exception> { ex.InnerException };
                }
            }

            return innerExceptions;
        }
    }

}