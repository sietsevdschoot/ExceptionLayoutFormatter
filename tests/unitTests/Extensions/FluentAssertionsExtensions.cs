using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Equivalency;

namespace UnitTests.Extensions
{
    public static class FluentAssertionExtensions
    {
        public static void BeEquivalentUsingWildcards(this StringCollectionAssertions assertion, IEnumerable<string> expectation,
            Func<EquivalencyAssertionOptions<string>, EquivalencyAssertionOptions<string>> config,
            string because = "",
            params object[] becauseArgs)
        {
            var subject = assertion.Subject;

            var newConfig = new Func<EquivalencyAssertionOptions<string>, EquivalencyAssertionOptions<string>>(cfg =>
            {
                var userConfig = config(cfg);
                return userConfig.Using(new CompareStringsUsingCaseInsensitiveWildcards());
            });

            subject.Should().BeEquivalentTo(expectation, newConfig, because, becauseArgs);
        }
    }

    public class CompareStringsUsingCaseInsensitiveWildcards : IEquivalencyStep
    {
        public EquivalencyResult Handle(Comparands comparands, IEquivalencyValidationContext context,
            IEquivalencyValidator nestedValidator)
        {
            if (comparands.GetExpectedType(new EquivalencyAssertionOptions()) != typeof(string))
            {
                return EquivalencyResult.ContinueWithNext;
            }
            
            var subject = (string)comparands.Subject;
            var expectation = (string)comparands.Expectation;

            subject.Should().MatchEquivalentOf(expectation, context.Reason.FormattedMessage, context.Reason.Arguments);

            return EquivalencyResult.AssertionCompleted;
        }
    }
}