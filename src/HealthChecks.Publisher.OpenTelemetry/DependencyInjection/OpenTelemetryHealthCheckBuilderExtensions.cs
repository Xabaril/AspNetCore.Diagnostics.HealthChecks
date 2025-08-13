using HealthChecks.Publisher.OpenTelemetry;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenTelemetryHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddOpenTelemetryPublisher(this IHealthChecksBuilder builder)
    {
        builder.Services.AddSingleton<IHealthCheckPublisher, OpenTelemetryPublisher>();
        builder.Services.AddOpenTelemetry().WithMetrics(x => x.AddMeter(OpenTelemetryPublisher.METER_NAME));
        return builder;
    }
}
