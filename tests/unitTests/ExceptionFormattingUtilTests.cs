using System;
using ExceptionLayoutFormatter;
using FluentAssertions;
using Xunit;

namespace ExceptionLayoutFormatterTests
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
        public void SetLayout_SettingsNullThrows()
        {
            var action = new Action(() => RenderMessage(null));

            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SetLayout_SettingsDuplicatesThrows()
        {
            var action = new Action(() => RenderMessage("{message}\n\n{message}"));

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
            var formatter = new ExceptionFormattingUtil();
            formatter.SetLayout(layout);

            string formattedMessage;

            try
            {
                throw new Exception(message);
            }
            catch (Exception ex)
            {
                formattedMessage = formatter.GetFormattedException(ex, additionalInfo);
            }

            return formattedMessage;
        }
    }
}