using HealthChecks.Uris;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class UrisHealthCheckBuilderExtensions
    {
        const string NAME = "uri-group";


        /// <summary>
        /// Add a health check for single uri.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uri">The uri to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="configureClient">An optional setup action to configure the Uris HealthCheck http client</param>
        /// <param name="configureHttpMessageHandler">An optional setup action to configure the Uris HealthCheck http client message handler</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri, string name = default,
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default, 
            Action<IServiceProvider, HttpClient> configureClient = null, Func<IServiceProvider, HttpMessageHandler> configureHttpMessageHandler = null)
        {
            var registrationName = name ?? NAME;
            ConfigureUrisClient(builder, configureClient, configureHttpMessageHandler, registrationName);

            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions()
                        .AddUri(uri);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>
        /// Add a health check for single uri.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uri">The uri to check.</param>
        /// <param name="httpMethod">The http method to use on check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="configureClient">An optional setup action to configure the Uris HealthCheck http client</param>
        /// <param name="configureHttpMessageHandler">An optional setup action to configure the Uris HealthCheck http client message handler</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri, HttpMethod httpMethod, string name = default,
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default,
            Action<IServiceProvider, HttpClient> configureClient = null, Func<IServiceProvider, HttpMessageHandler> configureHttpMessageHandler = null)
        {
            var registrationName = name ?? NAME;
            ConfigureUrisClient(builder, configureClient, configureHttpMessageHandler, registrationName);

            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions()
                        .AddUri(uri)
                        .UseHttpMethod(httpMethod);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>
        /// Add a health check for multiple uri's.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uris">The collection of uri's to be checked.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="configureClient">An optional setup action to configure the Uris HealthCheck http client</param>
        /// <param name="configureHttpMessageHandler">An optional setup action to configure the Uris HealthCheck http client message handler</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, IEnumerable<Uri> uris, string name = default,
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default,
            Action<IServiceProvider, HttpClient> configureClient = null, Func<IServiceProvider, HttpMessageHandler> configureHttpMessageHandler = null)
        {
            var registrationName = name ?? NAME;
            ConfigureUrisClient(builder, configureClient, configureHttpMessageHandler, registrationName);

            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp => CreateHealthCheck(sp, registrationName, UriHealthCheckOptions.CreateFromUris(uris)),
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>
        /// Add a health check for multiple uri's.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uris">The collection of uri's to be checked.</param>
        /// <param name="httpMethod">The http method to be used.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="configureClient">An optional setup action to configure the Uris HealthCheck http client</param>
        /// <param name="configureHttpMessageHandler">An optional setup action to configure the Uris HealthCheck http client message handler</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, IEnumerable<Uri> uris, HttpMethod httpMethod, string name = default,
            HealthStatus? failureStatus = default, IEnumerable<string> tags = default, TimeSpan? timeout = default,
            Action<IServiceProvider, HttpClient> configureClient = null, Func<IServiceProvider, HttpMessageHandler> configureHttpMessageHandler = null)
        {
            var registrationName = name ?? NAME;
            ConfigureUrisClient(builder, configureClient, configureHttpMessageHandler, registrationName);

            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = UriHealthCheckOptions
                        .CreateFromUris(uris)
                        .UseHttpMethod(httpMethod);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>
        /// Add a health check for multiple uri's.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uriOptions">The action used to configured uri values and specified http methods to be checked.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus">
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// </param>
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="configureClient">An optional setup action to configure the Uris HealthCheck http client</param>
        /// <param name="configureHttpMessageHandler">An optional setup action to configure the Uris HealthCheck http client message handler</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Action<UriHealthCheckOptions> uriOptions, string name = default, HealthStatus? failureStatus = default,
            IEnumerable<string> tags = default, TimeSpan? timeout = default,
            Action<IServiceProvider, HttpClient> configureClient = null, Func<IServiceProvider, HttpMessageHandler> configureHttpMessageHandler = null)
        {

            var registrationName = name ?? NAME;
            ConfigureUrisClient(builder, configureClient, configureHttpMessageHandler, registrationName);

            return builder.Add(new HealthCheckRegistration(
                registrationName,
                sp =>
                {
                    var options = new UriHealthCheckOptions();
                    uriOptions?.Invoke(options);

                    return CreateHealthCheck(sp, registrationName, options);
                },
                failureStatus,
                tags,
                timeout));
        }
        /// <summary>
        /// Add a health check for single uri.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/>.</param>
        /// <param name="uriProvider">Factory for providing the uri to check.</param>
        /// <param name="name">The health check name. Optional. If <c>null</c> the type name 'uri-group' will be used for the name.</param>
        /// <param name="failureStatus"></param>
        /// The <see cref="HealthStatus"/> that should be reported when the health check fails. Optional. If <c>null</c> then
        /// the default status of <see cref="HealthStatus.Unhealthy"/> will be reported.
        /// <param name="tags">A list of tags that can be used to filter sets of health checks. Optional.</param>
        /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
        /// <param name="configureClient">An optional setup action to configure the Uris HealthCheck http client</param>
        /// <param name="configureHttpMessageHandler">An optional setup action to configure the Uris HealthCheck http client message handler</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddUrlGroup(
            this IHealthChecksBuilder builder,
            Func<IServiceProvider, Uri> uriProvider,
            string name = null,
            HealthStatus? failureStatus = null,
            IEnumerable<string> tags = null,
            TimeSpan? timeout = null,
            Action<IServiceProvider, HttpClient> configureClient = null, Func<IServiceProvider, HttpMessageHandler> configureHttpMessageHandler = null)
        {
            var registrationName = name ?? NAME;
            ConfigureUrisClient(builder, configureClient, configureHttpMessageHandler, registrationName);

            return builder.Add(
                new HealthCheckRegistration(
                    registrationName,
                    sp =>
                    {
                        var uri = uriProvider(sp);
                        var uriHealthCheckOptions = new UriHealthCheckOptions().AddUri(uri, null);

                        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

                        return new UriHealthCheck(
                            uriHealthCheckOptions,
                            () => httpClientFactory.CreateClient(registrationName));
                    },
                    failureStatus,
                    tags,
                    timeout));
        }
        private static UriHealthCheck CreateHealthCheck(IServiceProvider sp, string name, UriHealthCheckOptions options)
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new UriHealthCheck(options, () => httpClientFactory.CreateClient(name));
        }


        static Action<IServiceProvider, HttpClient> EmptyHttpClientCallback = (_, _) => { };
        static Func<IServiceProvider, HttpMessageHandler> DefaultHttpMessageHandlerCallback = _ => new HttpClientHandler();

        private static void ConfigureUrisClient(IHealthChecksBuilder builder, Action<IServiceProvider, HttpClient> configureHttpclient, Func<IServiceProvider, HttpMessageHandler> configureHttpMessageHandler, string registrationName)
        {
            builder.Services.AddHttpClient(registrationName)
                .ConfigureHttpClient(configureHttpclient ?? EmptyHttpClientCallback)
                .ConfigurePrimaryHttpMessageHandler(configureHttpMessageHandler ?? DefaultHttpMessageHandlerCallback);
        }

    }
}
