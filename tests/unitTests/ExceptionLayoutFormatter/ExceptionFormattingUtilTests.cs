using System;
using System.Runtime.Serialization;
using ExceptionLayoutFormatter;
using FluentAssertions;
using Xunit;

namespace UnitTests.ExceptionLayoutFormatter
{
    public class ExceptionFormattingUtilTests
    {
        [Fact]
        public void GetFormattedException_CanRenderLayout()
        {
            var actual = RenderMessage("[${exceptionType}: ${message}]");

            actual.Should().Be("[Exception: Hello World!]");
        }

        [Fact]
        public void GetFormattedException_RightTrimsNewLinesForKeywordsWithNullValue()
        {
            var actual = RenderMessage("Message\n\n${additionalInfo}\n\n${message}");

            actual.Should().Be("Message\n\nHello World!");
        }

        [Fact]
        public void GetFormattedException_serializes_dictionary_if_available()
        {
            // Arrange
            var ex = new ArgumentException("MyTest");
            ex.Data["MyObject"] = new Person
            {
                Name = "MyTest",
                Address = "MyAddress",
                LuckyNumbers = new[] { 10, 11, 12 }
            };

            // Act
            var actual = RenderMessage(ex, "${message}\n\n${dictionary}");

            // Assert
            actual.Should().MatchEquivalentOf("*MyTest*MyAddress*");
        }

        [Fact]
        public void GetFormattedException_can_serializes_dictionary_with_null_value()
        {
            // Arrange
            var ex = new ArgumentException("MyTest");
            ex.Data["MyObject"] = null;

            // Act
            var actual = RenderMessage(ex, "${message}\n\n${dictionary}");

            // Assert
            actual.Should().MatchEquivalentOf("*MyTest*MyObject*");
        }

        [Fact]
        public void GetFormattedException_does_not_serialize_empty_dictionary()
        {
            // Arrange
            var ex = new ArgumentException("MyTest");
            ex.Data.Clear();

            // Act
            var actual = RenderMessage(ex, "${message}\n\n${dictionary}");

            // Assert
            actual.Should().NotMatchEquivalentOf("*{}*");
        }

        [Fact]
        public void SetLayout_SettingsNullThrows()
        {
            var action = new Action(() => RenderMessage(null));

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetLayout_SettingsDuplicatesThrows()
        {
            var action = new Action(() => RenderMessage("${message}\n\n${message}"));

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetLayout_SettingsUnknownThrows()
        {
            var action = new Action(() => RenderMessage("${message}\n\n${unknownKeyword}"));

            action.Should()
                .Throw<ArgumentException>()
                .WithMessage("*unknownKeyword*");
        }

        private string RenderMessage(string layout, string message = "Hello World!", string additionalInfo = null)
        {
            return RenderMessage(new Exception(message), layout, additionalInfo);
        }

        private string RenderMessage(Exception exception, string layout, string additionalInfo = null)
        {
            var formatter = new ExceptionFormattingUtil();
            formatter.SetLayout(layout);

            string formattedMessage;

            try
            {
                throw exception;
            }
            catch (Exception ex)
            {
                formattedMessage = formatter.GetFormattedException(ex, additionalInfo);
            }

            return formattedMessage;
        }
    }

    [Serializable]
    internal class Person
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int[] LuckyNumbers { get; set; }
    }

}