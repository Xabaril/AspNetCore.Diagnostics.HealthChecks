using HealthChecks.IdSvr;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to configure <see cref="IdSvrHealthCheck"/>.
    /// </summary>
    public static class IdSvrHealthCheckBuilderExtensions
    {
        private const string NAME = "idsvr";

        /// <summary>
        /// Add a health check for Identity Server.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="idSvrUri">The uri of the Identity Server to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'idsvr' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddIdentityServer(
            this IHealthChecksBuilder builder,
            Uri idSvrUri,
            string? name = default,
            HealthStatus? failureStatus = default,
            IEnumerable<string>? tags = default,
            TimeSpan? timeout = default)
        {
            var registrationName = name ?? NAME;

            builder.Services.AddHttpClient(registrationName, client => client.BaseAddress = idSvrUri);

            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp => new IdSvrHealthCheck(() => sp.GetRequiredService<IHttpClientFactory>().CreateClient(registrationName)),
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>
        /// Add a health check for Identity Server.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uriProvider">Factory for providing the uri of the Identity Server to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'idsvr' will be used for the name.</param>
        /// <param name="failureStatus"></param>
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional <see cref="TimeSpan"/> representing the timeout of the check.</param>
        /// <returns>The specified <paramref name="builder"/>.</returns>
        public static IHealthChecksBuilder AddIdentityServer(
            this IHealthChecksBuilder builder,
            Func<IServiceProvider, Uri> uriProvider,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null)
        {
            builder.Services.AddHttpClient();
            var registrationName = name ?? NAME;

            return builder.Add(
                new HealthCheckRegistration(
                    registrationName,
                    sp => new IdSvrHealthCheck(() => CreateIdentityServerHttpClient(sp, registrationName, uriProvider)),
                    failureStatus,
                    tags,
                    timeout));
        }


        private static HttpClient CreateIdentityServerHttpClient(IServiceProvider sp, string registrationName, Func<IServiceProvider, Uri> uriProvider)
        {
            var authorityUri = uriProvider(sp);
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient(registrationName);
            client.BaseAddress = authorityUri;

            return client;
        }
    }
}
