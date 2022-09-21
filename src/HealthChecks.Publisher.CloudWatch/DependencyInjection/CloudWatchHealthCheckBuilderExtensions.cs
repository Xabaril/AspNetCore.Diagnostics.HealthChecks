using Amazon;
using HealthChecks.Publisher.CloudWatch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

public static class CloudWatchHealthCheckBuilderExtensions
{
    /// <summary>
    /// Add a health check publisher for AWS CloudWatch.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a new event <c>AspNetCoreHealthCheck</c> is sent to AWS CloudWatch with two metrics <c>AspNetCoreHealthCheckStatus</c> and <c>AspNetCoreHealthCheckDuration</c>
    /// indicating the health check status ( 1 - Healthy 0 - Unhealthy) and the total time the health check took to execute on milliseconds.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCloudWatchPublisher(this IHealthChecksBuilder builder)
    {
        builder.Services.AddSingleton<IHealthCheckPublisher, CloudWatchPublisher>();

        return builder;
    }

    /// <summary>
    /// Add a health check publisher for AWS CloudWatch.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a new event <c>AspNetCoreHealthCheck</c> is sent to AWS CloudWatch with two metrics <c>AspNetCoreHealthCheckStatus</c> and <c>AspNetCoreHealthCheckDuration</c>
    /// indicating the health check status ( 1 - Healthy 0 - Unhealthy) and the total time the health check took to execute in milliseconds.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="serviceCheckName">The <see cref="string"/>.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCloudWatchPublisher(
        this IHealthChecksBuilder builder,
        string serviceCheckName)
    {
        builder.Services
           .AddSingleton<IHealthCheckPublisher, CloudWatchPublisher>(sp => new CloudWatchPublisher(serviceCheckName));

        return builder;
    }

    /// <summary>
    /// Add a health check publisher for AWS CloudWatch.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a new event <c>AspNetCoreHealthCheck</c> is sent to AWS CloudWatch with two metrics <c>AspNetCoreHealthCheckStatus</c> and <c>AspNetCoreHealthCheckDuration</c>
    /// indicating the health check status ( 1 - Healthy 0 - Unhealthy)  and the total time the health check took to execute on milliseconds.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="awsAccessKeyId">The <see cref="string"/>.</param>
    /// <param name="awsSecretAccessKey">The <see cref="string"/>.</param>
    /// <param name="region">The <see cref="RegionEndpoint"/>.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCloudWatchPublisher(
        this IHealthChecksBuilder builder,
        string awsAccessKeyId,
        string awsSecretAccessKey,
        RegionEndpoint region)
    {
        builder.Services
           .AddSingleton<IHealthCheckPublisher, CloudWatchPublisher>(sp => new CloudWatchPublisher(awsAccessKeyId, awsSecretAccessKey, region));

        return builder;
    }

    /// <summary>
    /// Add a health check publisher for AWS CloudWatch.
    /// </summary>
    /// <remarks>
    /// For each <see cref="HealthReport"/> published a new event <c>AspNetCoreHealthCheck</c> is sent to AWS CloudWatch with two metrics <c>AspNetCoreHealthCheckStatus</c> and <c>AspNetCoreHealthCheckDuration</c>
    /// indicating the health check status ( 1 - Healthy 0 - Unhealthy) and the total time the health check took to execute on milliseconds.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="serviceCheckName">The <see cref="string"/>.</param>
    /// <param name="awsAccessKeyId">The <see cref="string"/>.</param>
    /// <param name="awsSecretAccessKey">The <see cref="string"/>.</param>
    /// <param name="region">The <see cref="RegionEndpoint"/>.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddCloudWatchPublisher(
        this IHealthChecksBuilder builder,
        string serviceCheckName,
        string awsAccessKeyId,
        string awsSecretAccessKey,
        RegionEndpoint region)
    {
        builder.Services
           .AddSingleton<IHealthCheckPublisher, CloudWatchPublisher>(sp => new CloudWatchPublisher(serviceCheckName, awsAccessKeyId, awsSecretAccessKey, region));

        return builder;
    }
}
