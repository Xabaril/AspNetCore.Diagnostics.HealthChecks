namespace HealthChecks.Prometheus.Metrics.Tests.Functional;

public static class StringAssertionExentions
{
    public static string ShouldContainCheckAndResult(this string value, string checkName, HealthStatus expectedResult)
    {
        value.ShouldContain($"healthcheck{{healthcheck=\"{checkName}\"}} {expectedResult:D}",
            customMessage: "health check  must be included");
        value.ShouldContain($"healthcheck_duration_seconds{{healthcheck=\"{checkName}\"}}",
            customMessage: "health check duration must be included");
        return value;
    }
}
