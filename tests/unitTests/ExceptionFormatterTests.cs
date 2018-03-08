using System;
using System.Collections.Generic;
using ExceptionLayoutFormatter;
using ExceptionLayoutFormatter.ExceptionLayouts;
using ExceptionLayoutFormatterTests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace ExceptionLayoutFormatterTests
{
    public class ExceptionFormatterTests
    {
        [Fact]
        public void FormatException_Creates_exceptionMessage_innerException_first_outerException_last()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create();

            var ex = new ArgumentException("Ex1", new Exception("Ex2", new Exception("Ex3")));

            // Act
            var actual = formatter.FormatException(ex);

            // Assert
            actual.Should().MatchEquivalentOf("*Ex3*Ex2*Ex1*");
        }

        [Fact]
        public void FormatException_uses_matching_layout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create(new Dictionary<Type, IExceptionLayout>
            {
                { typeof(DummyException), new DummyExceptionLayout()},
                { typeof(Exception), new EmptyExceptionLayout()},
            });

            // Act
            var actual = formatter.FormatException(new DummyException());

            // Assert
            actual.Should().MatchEquivalentOf("DummyExceptionLayout*");
        }

        [Fact]
        public void FormatException_when_exceptionType_not_found_uses_baseType_Layout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create(new Dictionary<Type, IExceptionLayout>
            {
                { typeof(DummyException), new DummyExceptionLayout()}
            });

            // Act
            var subClassException = new SubClassedDummyException();
            var actual = formatter.FormatException(subClassException);

            // Assert
            actual.Should().MatchEquivalentOf("DummyExceptionLayout*");
        }

        [Fact]
        public void FormatException_when_exceptionType_not_found_uses_default_exception_layout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create(new Dictionary<Type, IExceptionLayout>
            {
                { typeof(Exception), new EmptyExceptionLayout()},
            });

            // Act
            var actual = formatter.FormatException(new SubClassedDummyException());

            // Assert
            actual.Should().MatchEquivalentOf("EmptyExceptionLayout*");
        }

        [Fact]
        public void FormatException_Uses_formatters_for_exceptionTypes()
        {
            var formatter = ExceptionFormatter.Create(new Dictionary<Type, IExceptionLayout>
            {
                { typeof(Exception), new EmptyExceptionLayout()},
                { typeof(DummyException), new DummyExceptionLayout()}
            });
            
            var ex = new AggregateException(new SubClassedDummyException("", new DummyException("", new Exception())));

            var names = formatter.FormatException(ex);

            names
                .Split(new []{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Should().ContainInOrder
            (
                "EmptyExceptionLayout",
                "DummyExceptionLayout",
                "DummyExceptionLayout",
                "EmptyExceptionLayout"
            );
        }

        [Fact]
        public void FormatException_serializes_dictionary_if_available()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create();

            var ex = new ArgumentException("MyTest");
            ex.Data["MyObject"] = new Person
            {
                Name = "MyTest",
                Address = "MyAddress",
                LuckyNumbers = new[] { 10, 11, 12 }
            };

            // Act
            var actual = formatter.FormatException(ex);

            // Assert
            actual.Should().MatchEquivalentOf("*MyTest*MyAddress*");
        }

        [Fact]
        public void FormatException_can_serializes_dictionary_with_null_value()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create();

            var ex = new ArgumentException("MyTest");
            ex.Data["MyObject"] = null;

            // Act
            var actual = formatter.FormatException(ex);

            // Assert
            actual.Should().MatchEquivalentOf("*MyTest*MyObject*");
        }

        [Serializable]
        internal class Person
        {
            public string Name { get; set; }
            public string Address { get; set; }
            public int[] LuckyNumbers { get; set; }
        }

    }
}