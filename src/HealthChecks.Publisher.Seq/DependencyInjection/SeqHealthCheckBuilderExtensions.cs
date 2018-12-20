using HealthChecks.Publisher.Seq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SeqHealthCheckBuilderExtensions
    {
        const string NAME = "seq";

        public static IHealthChecksBuilder AddSeqPublisher(this IHealthChecksBuilder builder, Action<SeqOptions> setup, string name = default)
        {
            var options = new SeqOptions();
            setup(options);

            var registrationName = name ?? NAME;
            builder.Services.AddHttpClient(registrationName, client => client.BaseAddress = new Uri(options.Endpoint));

            builder.Services.AddSingleton<IHealthCheckPublisher>(sp =>
            {
                return new SeqPublisher(() => sp.GetRequiredService<IHttpClientFactory>().CreateClient(registrationName), options);
            });

            return builder;
        }
    }
}
