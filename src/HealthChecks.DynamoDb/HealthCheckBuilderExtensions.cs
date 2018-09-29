using HealthChecks.DynamoDb;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HealthCheckBuilderExtensions
    {
        const string NAME = "dynamodb";

        public static IHealthChecksBuilder AddDynamoDb(this IHealthChecksBuilder builder, Action<DynamoDBOptions> setup)
        {
            var options = new DynamoDBOptions();
            setup?.Invoke(options);


            return builder.Add(new HealthCheckRegistration(
                NAME,
                sp => new DynamoDbHealthCheck(options, sp.GetService<ILogger<DynamoDbHealthCheck>>()),
                null,
                new string[] { NAME }));
        }
    }
}
