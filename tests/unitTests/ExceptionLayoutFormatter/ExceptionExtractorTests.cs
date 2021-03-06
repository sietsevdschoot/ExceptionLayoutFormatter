﻿using System;
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
    
    public class ExceptionExtractorTests
    {
        private readonly ExceptionExtractor _extractor;

        public ExceptionExtractorTests()
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
        public void ExtractAllExceptions_Can_extract_nested_innerExceptions()
        {
            // Arrange
            var ex = new AggregateException("OuterAggregate", new List<Exception>
            {
                new AggregateException("Inner1", new List<Exception>
                {
                    new Exception("Inner1A"),
                    new Exception("Inner1B"),

                }),
                new AggregateException("Inner2", new List<Exception>
                {
                    new Exception("Inner2A"),
                    new Exception("Inner2B"),

                }),
                new AggregateException("Inner3", new List<Exception>
                {
                    new Exception("Inner3A"),
                    new Exception("Inner3B"),

                }),
            });

            // Act
            var actual = _extractor.ExtractAllExceptions(ex).Select(x => x.Message).ToList();

            // Assert
            actual.Should().BeEquivalentUsingWildcards(new List<string>
            {
                "Inner3B",
                "Inner3A",
                "Inner3 *",
                "Inner2B",
                "Inner2A",
                "Inner2 *",
                "Inner1B",
                "Inner1A",
                "Inner1 *",
                "OuterAggregate*",
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