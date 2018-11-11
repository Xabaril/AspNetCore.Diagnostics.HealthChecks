using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FunctionalTests.HealthChecks.Publisher.Prometheus
{
    public static class StringAssertionExentions
    {
        public static AndConstraint<StringAssertions> ContainCheckAndResult(this StringAssertions assertions, string checkName,
            HealthStatus expectedResult)
        {

            assertions.Contain($"liveness{{healthcheck=\"{checkName}\"}} {expectedResult:D}",
                "health check liveness must be included");
            assertions.Contain($"liveness_duration_seconds{{healthcheck=\"{checkName}\"}}",
                "health check duration must be included");
            return new AndConstraint<StringAssertions>(assertions);
        }
    }
}