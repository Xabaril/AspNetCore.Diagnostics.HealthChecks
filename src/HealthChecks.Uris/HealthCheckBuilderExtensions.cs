using HealthChecks.Uris;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string name = "uri-group";

        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri)
        {
            var options = new UriHealthCheckOptions();
            options.AddUri(uri);

            return builder.Add(new HealthCheckRegistration(
                name,
                sp => new UriHealthCheck(options, sp.GetService<ILogger<UriHealthCheck>>()),
                null,
                new string[] { name }));
        }

        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Uri uri, HttpMethod httpMethod)
        {
            var options = new UriHealthCheckOptions();
            options.AddUri(uri);
            options.UseHttpMethod(httpMethod);

            return builder.Add(new HealthCheckRegistration(
                name,
                sp => new UriHealthCheck(options, sp.GetService<ILogger<UriHealthCheck>>()),
                null,
                new string[] { name }));
        }

        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, IEnumerable<Uri> uris)
        {
            var options = UriHealthCheckOptions.CreateFromUris(uris);

            return builder.Add(new HealthCheckRegistration(
                name,
                sp => new UriHealthCheck(options, sp.GetService<ILogger<UriHealthCheck>>()),
                null,
                new string[] { name }));
        }

        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, IEnumerable<Uri> uris, HttpMethod httpMethod)
        {
            var options = UriHealthCheckOptions.CreateFromUris(uris);
            options.UseHttpMethod(httpMethod);

            return builder.Add(new HealthCheckRegistration(
                name,
                sp => new UriHealthCheck(options, sp.GetService<ILogger<UriHealthCheck>>()),
                null,
                new string[] { name }));
        }

        public static IHealthChecksBuilder AddUrlGroup(this IHealthChecksBuilder builder, Action<UriHealthCheckOptions> uriOptions)
        {
            var options = new UriHealthCheckOptions();
            uriOptions?.Invoke(options);

            return builder.Add(new HealthCheckRegistration(
                name,
                sp => new UriHealthCheck(options, sp.GetService<ILogger<UriHealthCheck>>()),
                null,
                new string[] { name }));
        }
    }
}
