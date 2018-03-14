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
        public bool CanHandle(IEquivalencyValidationContext context, IEquivalencyAssertionOptions config)
        {
            Type expectationType = config.GetExpectationType(context);

            return (expectationType != null) && (expectationType == typeof(string));
        }

        public bool Handle(IEquivalencyValidationContext context, IEquivalencyValidator parent, IEquivalencyAssertionOptions config)
        {
            string subject = (string)context.Subject;
            string expectation = (string)context.Expectation;

            subject.Should().MatchEquivalentOf(expectation, context.Because, context.BecauseArgs);

            return true;
        }
    }
}