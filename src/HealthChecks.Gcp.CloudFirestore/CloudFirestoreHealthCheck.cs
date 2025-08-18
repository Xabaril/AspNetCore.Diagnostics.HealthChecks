using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Gcp.CloudFirestore;

public class CloudFirestoreHealthCheck : IHealthCheck
{
    private readonly CloudFirestoreOptions _cloudFirestoreOptions;

    public CloudFirestoreHealthCheck(CloudFirestoreOptions cloudFirestoreOptions)
    {
        _cloudFirestoreOptions = Guard.ThrowIfNull(cloudFirestoreOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var checkDetails = new Dictionary<string, object>{
            { "health_check.task", "ready" },
            { "db.system.name", "gcp.firestore" },
            { "network.transport", "tcp" }
        };

        try
        {
            checkDetails.Add("db.namespace", _cloudFirestoreOptions.FirestoreDatabase);
            var currentRootCollections = await GetRootCollectionsAsync(cancellationToken).ConfigureAwait(false);
            if (_cloudFirestoreOptions.RequiredCollections != null)
            {
                var inexistantCollections = _cloudFirestoreOptions.RequiredCollections
                    .Except(currentRootCollections);

                if (inexistantCollections.Any())
                {
                    return new HealthCheckResult(
                        context.Registration.FailureStatus,
                        description: "Collections not found: " + string.Join(", ", inexistantCollections.Select(c => "'" + c + "'")),
                        data: checkDetails
                    );
                }
            }

            return HealthCheckResult.Healthy(data: checkDetails);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: checkDetails);
        }
    }

    private async Task<List<string>> GetRootCollectionsAsync(CancellationToken cancellationToken)
    {
        var collections = new List<string>();

        await foreach (var item in _cloudFirestoreOptions.FirestoreDatabase.ListRootCollectionsAsync())
        {
            cancellationToken.ThrowIfCancellationRequested();
            collections.Add(item.Id);
        }

        return collections;
    }
}
