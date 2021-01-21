using System;
using ExceptionLayoutFormatter;
using FluentAssertions;
using UnitTests.TestHelpers;
using Xunit;

namespace UnitTests.ExceptionLayoutFormatter
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
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout<DummyExceptionLayout>()
                .AddExceptionLayout<SubClassedDummyExceptionLayout>()
                .AddExceptionLayout<EmptyExceptionLayout>();
            // Act
            var actual = formatter.FormatException(new SubClassedDummyException());

            // Assert
            actual.Should().MatchEquivalentOf("SubClassedDummyException*");
        }

        [Fact]
        public void FormatException_when_exceptionType_not_found_uses_baseType_Layout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout<DummyExceptionLayout>();

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
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout<EmptyExceptionLayout>();

            // Act
            var actual = formatter.FormatException(new SubClassedDummyException());

            // Assert
            actual.Should().MatchEquivalentOf("EmptyExceptionLayout*");
        }

        [Fact]
        public void FormatException_Uses_formatters_for_exceptionTypes()
        {
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout<DummyExceptionLayout>()
                .AddExceptionLayout<EmptyExceptionLayout>();

            var ex = new AggregateException(
                new SubClassedDummyException("", 
                    new DummyException("", new Exception())));

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

        [Fact]
        public void FormatException_can_handle_null_values()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create();

            // Act
            var actual = formatter.FormatException(null);

            // Assert
            actual.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Can_add_exceptionLayouts_by_scanning_assemblies()
        {
            // Arrange && Assert
            var formatter = ExceptionFormatter.Create(this.GetType().Assembly);

            // Assert
            formatter.ExceptionLayouts.Should().NotBeEmpty();
        }

        [Fact]
        public void AddExceptionLayout_adds_exceptionLayout_to_formatter()
        {
            // Arrange && Assert
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(new EmptyExceptionLayout())
                .AddExceptionLayout(new DummyExceptionLayout());

            // Assert
            formatter.ExceptionLayouts.Should().HaveCount(2);
        }

        [Fact]
        public void AddExceptionLayout_adds_exceptionLayout_to_formatter_using_generics()
        {
            // Arrange && Assert
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout<EmptyExceptionLayout>()
                .AddExceptionLayout<DummyExceptionLayout>();

            // Assert
            formatter.ExceptionLayouts.Should().HaveCount(2);
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