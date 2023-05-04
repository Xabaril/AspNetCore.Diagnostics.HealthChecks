using HealthChecks.Aws.SecretsManager;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="SecretsManagerHealthCheck"/>.
/// </summary>
public static class SecretsManagerHealthCheckBuilderExtensions
{
    private const string NAME = "aws secrets manager";

    /// <summary>
    /// Add a health check for AWS Secrets Manager.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The action to configure the Secrets Manager Configuration e.g. access key, secret key, region etc.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'aws secrets manager' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSecretsManager(
        this IHealthChecksBuilder builder,
        Action<SecretsManagerOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new SecretsManagerOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new SecretsManagerHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
