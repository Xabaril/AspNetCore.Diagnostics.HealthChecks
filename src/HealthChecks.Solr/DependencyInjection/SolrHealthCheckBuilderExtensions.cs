using HealthChecks.Solr;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="SolrHealthCheck"/>.
/// </summary>
public static class SolrHealthCheckBuilderExtensions
{
    private const string NAME = "solr";

    /// <summary>
    /// Add a health check for Solr databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="solrUri">The Solr connection string to be used.</param>
    /// <param name="solrCore"></param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'solr' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSolr(
        this IHealthChecksBuilder builder,
        string solrUri,
        string solrCore,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new SolrOptions();
        options.UseServer(solrUri, solrCore, username: null, password: null, timeout: null);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new SolrHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for solr databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The solr option setup.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'solr' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddSolr(
        this IHealthChecksBuilder builder,
        Action<SolrOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new SolrOptions();
        setup?.Invoke(options);

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new SolrHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }
}
