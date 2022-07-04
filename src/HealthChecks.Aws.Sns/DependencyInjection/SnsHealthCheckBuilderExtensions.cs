using HealthChecks.Aws.Sns;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="SnsTopicAndSubscriptionHealthCheck"/>.
/// </summary>
public static class SnsHealthCheckBuilderExtensions
{
    private const string AWS_NAME_SNS_SUBS = "aws sns subs";

    /// <summary>
    /// Add a health check for AWS SNS topics and optional subscriptions.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the SNS connection parameters.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'aws sns subs' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSnsTopicsAndSubscriptions(
        this IHealthChecksBuilder builder,
        Action<SnsOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new SnsOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? AWS_NAME_SNS_SUBS,
            sp => new SnsTopicAndSubscriptionHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
