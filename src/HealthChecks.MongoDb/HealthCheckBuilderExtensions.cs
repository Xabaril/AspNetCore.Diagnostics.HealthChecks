using HealthChecks.MongoDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "mongodb";

        public static IHealthChecksBuilder AddMongoDb(this IHealthChecksBuilder builder, string mongodbConnectionString)
        {
            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new MongoDbHealthCheck(mongodbConnectionString, sp.GetService<ILogger<MongoDbHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
