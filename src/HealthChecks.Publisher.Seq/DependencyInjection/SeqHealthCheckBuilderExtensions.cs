using HealthChecks.Publisher.Seq;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SeqHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddSeqPublisher(this IHealthChecksBuilder builder, Action<SeqOptions> setup)
        {
            var options = new SeqOptions();
            setup(options);

            builder.Services.AddSingleton<IHealthCheckPublisher>(sp =>
            {
                return new SeqPublisher(options);
            });

            return builder;
        }
    }
}
