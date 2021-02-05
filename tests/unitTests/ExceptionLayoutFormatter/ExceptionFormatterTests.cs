using System;
using System.ServiceModel;
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
                .AddExceptionLayout(new CustomerExceptionLayout())
                .AddExceptionLayout(new CustomerNotFoundExceptionLayout())
                .AddExceptionLayout(new DefaultExceptionLayout());

            // Act
            var actual = formatter.FormatException(new CustomerNotFoundException());

            // Assert
            actual.Should().MatchEquivalentOf("CustomerNotFoundException*");
        }

        [Fact]
        public void FormatException_when_exceptionType_not_found_uses_baseType_Layout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(new CustomerExceptionLayout());

            // Act
            var subClassException = new CustomerNotFoundException();
            var actual = formatter.FormatException(subClassException);

            // Assert
            actual.Should().MatchEquivalentOf("CustomerExceptionLayout*");
        }

        [Fact]
        public void FormatException_can_format_generic_exceptions_using_generic_exception_layout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(typeof(GenericFaultExceptionLayout<>));

            // Act
            var actual = formatter.FormatException(new FaultException<CalculationError>(new CalculationError
            {
                Reason = "MyReason"
            }));

            // Assert
            actual.Should().MatchEquivalentOf("*CalculationError*MyReason*");
        }

        [Fact]
        public void FormatException_when_exceptionType_not_found_uses_default_exception_layout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(new DefaultExceptionLayout());

            // Act
            var actual = formatter.FormatException(new CustomerNotFoundException());

            // Assert
            actual.Should().MatchEquivalentOf("DefaultExceptionLayout*");
        }

        [Fact]
        public void FormatException_Uses_formatters_for_exceptionTypes()
        {
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(new DefaultExceptionLayout())
                .AddExceptionLayout(new CustomerExceptionLayout())
                .AddExceptionLayout(new CustomerNotFoundExceptionLayout());

            var ex = new AggregateException(
                new CustomerNotFoundException("", 
                    new CustomerException("", 
                        new Exception(""))));

            var names = formatter.FormatException(ex);

            names.Should().MatchEquivalentOf(string.Join("*",
                "DefaultExceptionLayout",
                "CustomerExceptionLayout",
                "CustomerNotFoundExceptionLayout",
                "DefaultExceptionLayout*"
            ));
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
        public void FormatException_When_exception_in_layout_falls_back_to_defaultExceptionLayout()
        {
            // Arrange
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(new ExceptionThrowingLayout());

            // Act
            var actual = formatter.FormatException(new OrderException("Order failed"));

            // Assert
            actual.Should().MatchEquivalentOf("*An error* in *ExceptionThrowingLayout*falling back*Order failed*");
        }

        [Fact]
        public void Can_add_exceptionLayouts_by_scanning_assemblies()
        {
            // Arrange && Assert
            var formatter = ExceptionFormatter.Create(this.GetType().Assembly);

            // Assert
            ((ExceptionFormatter)formatter).ExceptionLayouts.Should().NotBeEmpty();
        }

        [Fact]
        public void AddExceptionLayout_adds_exceptionLayout_to_formatter()
        {
            // Arrange && Assert
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(new DefaultExceptionLayout())
                .AddExceptionLayout(new CustomerExceptionLayout())
                .AddExceptionLayout(new FaultExceptionLayout());

            // Assert
            formatter.ExceptionLayouts.Should().HaveCount(3);
        }
 
        [Fact]
        public void AddExceptionLayout_adds_exceptionLayout_to_formatter_by_type()
        {
            // Arrange && Assert
            var formatter = ExceptionFormatter.Create()
                .AddExceptionLayout(new DefaultExceptionLayout())
                .AddExceptionLayout(typeof(GenericFaultExceptionLayout<>));

            // Assert
            formatter.ExceptionLayouts.Should().HaveCount(2);
        }
    }
}