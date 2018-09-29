using HealthChecks.IdSvr;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "idsvr";

        public static IHealthChecksBuilder AddIdentityServer(this IHealthChecksBuilder builder, Uri idSvrUri)
        {
            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new IdSvrHealthCheck(idSvrUri, sp.GetService<ILogger<IdSvrHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
