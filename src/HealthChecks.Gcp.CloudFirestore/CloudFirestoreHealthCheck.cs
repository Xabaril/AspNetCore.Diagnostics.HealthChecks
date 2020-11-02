using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HealthChecks.Gcp.CloudFirestore
{
    public class CloudFirestoreHealthCheck : IHealthCheck
    {
        private readonly CloudFirestoreOptions _cloudFirestoreOptions;
        public CloudFirestoreHealthCheck(CloudFirestoreOptions cloudFirestoreOptions)
        {
            _cloudFirestoreOptions = cloudFirestoreOptions ?? throw new ArgumentNullException(nameof(cloudFirestoreOptions));
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentRootCollections = await GetRootCollectionsAsync(cancellationToken);
                if (_cloudFirestoreOptions.RequiredCollections != null)
                {
                    var inexistantCollections = _cloudFirestoreOptions.RequiredCollections
                        .Except(currentRootCollections);

                    if (inexistantCollections.Any())
                    {
                        return new HealthCheckResult(
                            context.Registration.FailureStatus,
                            description: "Collections not found: " + string.Join(", ", inexistantCollections.Select(c => "'" + c + "'"))
                        );
                    }
                }

                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
            }
        }
        private async Task<List<string>> GetRootCollectionsAsync(CancellationToken cancellationToken)
        {
            var collections = new List<string>();

            await foreach (var item in _cloudFirestoreOptions.FirestoreDatabase.ListRootCollectionsAsync())
            {
                collections.Add(item.Id);
            }

            return collections;
        }
    }
}
