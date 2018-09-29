using HealthChecks.DocumentDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "documentdb";

        public static IHealthChecksBuilder AddDocumentDb(this IHealthChecksBuilder builder, Action<DocumentDbOptions> setup)
        {
            var documentDbOptions = new DocumentDbOptions();
            setup?.Invoke(documentDbOptions);

            return builder.Add(new HealthCheckRegistration(
               NAME,
               sp => new DocumentDbHealthCheck(documentDbOptions, sp.GetService<ILogger<DocumentDbHealthCheck>>()),
               null,
               new string[] { NAME }));
        }
    }
}
