using HealthChecks.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="ElasticsearchHealthCheck"/>.
/// </summary>
public static class ElasticsearchHealthCheckBuilderExtensions
{
    private const string NAME = "elasticsearch";

    /// <summary>
    /// Add a health check for Elasticsearch databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="elasticsearchUri">The Elasticsearch connection string to be used.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'elasticsearch' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddElasticsearch(
        this IHealthChecksBuilder builder,
        string elasticsearchUri,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new ElasticsearchOptions();
        options.UseServer(elasticsearchUri);
        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new ElasticsearchHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Elasticsearch databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="setup">The Elasticsearch option setup.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'elasticsearch' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddElasticsearch(
        this IHealthChecksBuilder builder,
        Action<ElasticsearchOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var options = new ElasticsearchOptions();
        setup?.Invoke(options);

        options.RequestTimeout ??= timeout;

        if (options.Uri is null && !options.AuthenticateWithElasticCloud)
        {
            throw new InvalidOperationException($"there is no server to connect. consider using ${nameof(ElasticsearchOptions.UseElasticCloud)} or ${nameof(ElasticsearchOptions.UseServer)}");
        }

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp => new ElasticsearchHealthCheck(options),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for Elasticsearch databases.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="clientFactory">
    /// An optional factory to obtain <see cref="Elastic.Clients.Elasticsearch.ElasticsearchClient" /> instance.
    /// When not provided, <see cref="Elastic.Clients.Elasticsearch.ElasticsearchClient" /> is simply resolved from <see cref="IServiceProvider"/>.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'elasticsearch' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddElasticsearch(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, Elastic.Clients.Elasticsearch.ElasticsearchClient>? clientFactory = null,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {

        return builder.Add(new HealthCheckRegistration(
            name ?? NAME,
            sp =>
            {
                var options = new ElasticsearchOptions();
                options.RequestTimeout ??= timeout;
                options.Client ??= clientFactory?.Invoke(sp) ?? sp.GetRequiredService<Elastic.Clients.Elasticsearch.ElasticsearchClient>();
                return new ElasticsearchHealthCheck(options);
            },
            failureStatus,
            tags,
            timeout));
    }
}
