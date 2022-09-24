using HealthChecks.Publisher.CloudWatch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class CloudWatchHealthCheckBuilderExtensions
{
    /// <summary>
    /// Add a health check publisher for AWS CloudWatch.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a new event <c>AspNetCoreHealthCheck</c> is sent to AWS CloudWatch
    /// with two metrics <c>AspNetCoreHealthCheckStatus</c> and <c>AspNetCoreHealthCheckDuration</c> indicating the health
    /// check status (1 - Healthy, 0 - Unhealthy) and the total time the health check took to execute on milliseconds.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">Delegate to configure publisher options.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCloudWatchPublisher(this IHealthChecksBuilder builder, Action<CloudWatchOptions>? setup = null)
    {
        var options = new CloudWatchOptions();
        setup?.Invoke(options);

        builder.Services.AddSingleton<IHealthCheckPublisher>(sp => new CloudWatchPublisher(options));

        return builder;
    }
}
