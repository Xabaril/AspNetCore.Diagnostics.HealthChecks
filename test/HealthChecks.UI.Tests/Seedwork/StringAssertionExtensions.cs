using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.UI.Tests
{
    public static class StringAssertionExentions
    {
        public static AndConstraint<StringAssertions> ContainCheckAndResult(this StringAssertions assertions, string checkName,
            HealthStatus expectedResult)
        {

            assertions.Contain($"healthcheck{{healthcheck=\"{checkName}\"}} {expectedResult:D}",
                "health check  must be included");
            assertions.Contain($"healthcheck_duration_seconds{{healthcheck=\"{checkName}\"}}",
                "health check duration must be included");
            return new AndConstraint<StringAssertions>(assertions);
        }
    }
}
