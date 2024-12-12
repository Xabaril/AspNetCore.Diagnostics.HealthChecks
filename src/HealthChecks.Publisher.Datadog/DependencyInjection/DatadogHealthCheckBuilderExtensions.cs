using HealthChecks.Publisher.Datadog;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StatsdClient;

namespace Microsoft.Extensions.DependencyInjection;

public static class DatadogHealthCheckBuilderExtensions
{
    /// <summary>
    /// Add a health check publisher for Datadog.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a custom service check indicating the health check status (OK - Healthy, WARNING - Degraded, CRITICAL - Unhealthy)
    /// and a metric indicating the total time the health check took to execute in milliseconds is sent to Datadog.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="serviceCheckName">Specifies the name of the custom check and metric that will be published to Datadog. Example: "myservice.healthchecks".</param>
    /// <param name="serviceFactory">
    /// An optional factory to obtain <see cref="DogStatsdService"/> used by the health check.
    /// When not provided, <see cref="DogStatsdService" /> is simply resolved from <see cref="IServiceProvider"/>.
    /// </param>
    /// <param name="defaultTags">Specifies a collection of tags to send with the custom check and metric.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDatadogPublisher(
        this IHealthChecksBuilder builder,
        string serviceCheckName,
        Func<IServiceProvider, DogStatsdService>? serviceFactory = default,
        string[]? defaultTags = default)
    {
        builder.Services
            .AddSingleton<IHealthCheckPublisher>(sp =>
            {
                DogStatsdService service = serviceFactory?.Invoke(sp) ?? sp.GetRequiredService<DogStatsdService>();

                return new DatadogPublisher(service, serviceCheckName, defaultTags);
            });

        return builder;
    }

    /// <summary>
    /// Add a health check publisher for Datadog.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a custom service check indicating the health check status (OK - Healthy, WARNING - Degraded, CRITICAL - Unhealthy)
    /// and a metric indicating the total time the health check took to execute in milliseconds is sent to Datadog.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="serviceCheckName">Specifies the name of the custom check and metric that will be published to Datadog. Example: "myservice.healthchecks".</param>
    /// <param name="statsdConfigFactory">The factory method to resolve <see cref="StatsdConfig"/>.</param>
    /// <param name="defaultTags">Specifies a collection of tags to send with the custom check and metric.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddDatadogPublisher(
        this IHealthChecksBuilder builder,
        string serviceCheckName,
        Func<IServiceProvider, StatsdConfig> statsdConfigFactory,
        string[]? defaultTags = default)
    {
        builder.Services
            .AddSingleton<IHealthCheckPublisher>(sp =>
            {
                var dogStatsdService = new DogStatsdService();

                dogStatsdService.Configure(statsdConfigFactory.Invoke(sp));

                return new DatadogPublisher(dogStatsdService, serviceCheckName, defaultTags);
            });

        return builder;
    }
}
