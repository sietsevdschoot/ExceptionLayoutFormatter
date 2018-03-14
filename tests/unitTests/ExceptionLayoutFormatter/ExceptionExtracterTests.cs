using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using ExceptionLayoutFormatter;
using FluentAssertions;
using UnitTests.Extensions;
using Xunit;

namespace UnitTests.ExceptionLayoutFormatter
{
    
    public class ExceptionExtracterTests
    {
        private readonly ExceptionExtractor _extractor;

        public ExceptionExtracterTests()
        {
            _extractor = new ExceptionExtractor();
        }

        [Fact]
        public void ExtractAllExceptions_Extracts_all_exceptions_inner_first_outer_last()
        {
            // Arrange
            var ex = new Exception("Ex1", new Exception("Ex2", new Exception("Ex3")));

            // Act
            var actual = _extractor.ExtractAllExceptions(ex);

            // Assert
            actual.Select(x => x.Message).Should().ContainInOrder(new List<string>
            {
                "Ex3",
                "Ex2",
                "Ex1",
            });
        }

        [Fact]
        public void ExtractAllExceptions_Can_extract_innerExceptions_from_AggregateException()
        {
            // Arrange
            var ex = new AggregateException("Aggregate", new List<Exception>
            {
                new Exception("Inner 1"),
                new Exception("Inner 2"),
                new Exception("Inner 3"),
            });

            // Act
            var actual = _extractor.ExtractAllExceptions(ex);

            // Assert
            actual.Select(x => x.Message).Should().BeEquivalentUsingWildcards(new List<string>
            {
                "Inner 3",
                "Inner 2",
                "Inner 1",
                "Aggregate*",
            }, 
            config => config.WithStrictOrderingFor(x => x));
        }

        [Fact]
        public void ExtractAllExceptions_Can_extract_innerExceptions_from_ReflectionTypeLoadException()
        {
            // Arrange
            var ex = new ReflectionTypeLoadException(new Type[0], new[]
            {
                new Exception("Inner 1"),
                new Exception("Inner 2"),
                new Exception("Inner 3"),
            });

            // Act
            var actual = _extractor.ExtractAllExceptions(ex);

            // Assert
            actual.Should().HaveCount(4);
        }

        [Fact]
        public void ExtractAllExceptions_Can_extract_innerExceptions_from_SmtpFailedRecipientsException()
        {
            // Arrange
            var ex = new SmtpFailedRecipientsException("failed", new[]
            {
                new SmtpFailedRecipientException("Inner 1"),
                new SmtpFailedRecipientException("Inner 2"),
                new SmtpFailedRecipientException("Inner 3"),
            });

            // Act
            var actual = _extractor.ExtractAllExceptions(ex);

            // Assert
            actual.Should().HaveCount(4);
        }
    }
}