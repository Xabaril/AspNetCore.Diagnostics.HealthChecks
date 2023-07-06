using HealthChecks.Publisher.CloudWatch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class CloudWatchHealthCheckBuilderExtensions
{
    /// <summary>
    /// Add a health check publisher for AWS CloudWatch.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a new metric is sent to AWS CloudWatch indicating
    /// the health check status (2 - Healthy, 1 - Degraded, 0 - Unhealthy)
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
