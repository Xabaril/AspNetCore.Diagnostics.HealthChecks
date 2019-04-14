using HealthChecks.Gcp.CloudFirestore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CloudFirestoreHealthCheckBuilderExtensions
    {
        const string NAME = "cloud firestore";

        public static IHealthChecksBuilder AddCloudFirestore(this IHealthChecksBuilder builder, Action<CloudFirestoreOptions> setup, string name = default, HealthStatus? failureStatus = default, IEnumerable<string> tags = default)
        {
            var cloudFirestoreOptions = new CloudFirestoreOptions();
            setup?.Invoke(cloudFirestoreOptions);

            return builder.Add(new HealthCheckRegistration(
               name ?? NAME,
               sp => new CloudFirestoreHealthCheck(cloudFirestoreOptions),
               failureStatus,
               tags));
        }
    }
}
