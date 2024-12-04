using HealthChecks.OpenIdConnectServer;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure <see cref="OpenIdConnectServerHealthCheck"/>.
/// </summary>
public static class OpenIdConnectServerHealthCheckBuilderExtensions
{
    private const string NAME = "oidcserver";
    internal const string OIDC_SERVER_DISCOVER_CONFIGURATION_SEGMENT = ".well-known/openid-configuration";

    /// <summary>
    /// Add a health check for OpenID Connect server.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="oidcSvrUri">The uri of the OpenID Connect server to check.</param>
    /// <param name="discoverConfigurationSegment">OpenID Connect server discover configuration segment.</param>
    /// <param name="isDynamicOpenIdProvider">Set to true if the health check shall validate the existence of code, id_token, and the id_token token in the reponse type values.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'oidcserver' will be used for the name.</param>
    /// <param name="failureStatus">
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddOpenIdConnectServer(
        this IHealthChecksBuilder builder,
        Uri oidcSvrUri,
        string discoverConfigurationSegment = OIDC_SERVER_DISCOVER_CONFIGURATION_SEGMENT,
        bool isDynamicOpenIdProvider = true,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var registrationName = name ?? NAME;

        builder.Services.AddHttpClient(registrationName, client => client.BaseAddress = oidcSvrUri);

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp => new OpenIdConnectServerHealthCheck(() => sp.GetRequiredService<IHttpClientFactory>().CreateClient(registrationName), discoverConfigurationSegment, isDynamicOpenIdProvider),
            failureStatus,
            tags,
            timeout));
    }

    /// <summary>
    /// Add a health check for OpenID Connect server.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
    /// <param name="uriProvider">Factory for providing the uri of the OpenID Connect server to check.</param>
    /// <param name="discoverConfigurationSegment">OpenID Connect server discover configuration segment.</param>
    /// <param name="isDynamicOpenIdProvider">Set to true if the health check shall validate the existence of code, id_token, and the id_token token in the reponse type values.</param>
    /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'oidcserver' will be used for the name.</param>
    /// <param name="failureStatus"></param>
    /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
    /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
    /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
    /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
    /// <returns>The specified <paramref name="builder"/>.</returns>
    public static IHealthChecksBuilder AddOpenIdConnectServer(
        this IHealthChecksBuilder builder,
        Func<IServiceProvider, Uri> uriProvider,
        string discoverConfigurationSegment = OIDC_SERVER_DISCOVER_CONFIGURATION_SEGMENT,
        bool isDynamicOpenIdProvider = true,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        var registrationName = name ?? NAME;

        builder.Services.AddHttpClient(registrationName, (sp, client) => client.BaseAddress = uriProvider(sp));

        return builder.Add(new HealthCheckRegistration(
            registrationName,
            sp => new OpenIdConnectServerHealthCheck(() => sp.GetRequiredService<IHttpClientFactory>().CreateClient(registrationName), discoverConfigurationSegment, isDynamicOpenIdProvider),
            failureStatus,
            tags,
            timeout));
    }
}
