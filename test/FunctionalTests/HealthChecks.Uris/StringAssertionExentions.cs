using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FunctionalTests.HealthChecks.Uris
{
    public static class StringAssertionExentions
    {
        public static AndConstraint<StringAssertions> ContainCheckAndResult(this StringAssertions assertions, string url, int statusCode, string reason, string body, string verb)
        {

            assertions.Contain($"\"data\":{{\"url\":\"{url}\",\"status\":{statusCode},\"reason\":\"{reason}\",\"body\":\"{body}\",\"verb\":\"{verb}\"}}");
            return new AndConstraint<StringAssertions>(assertions);
        }
    }
}