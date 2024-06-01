using System.Collections.ObjectModel;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Gcp.CloudFirestore;

public class CloudFirestoreHealthCheck : IHealthCheck
{
    private readonly CloudFirestoreOptions _cloudFirestoreOptions;
    private readonly Dictionary<string, object> _baseCheckDetails = new Dictionary<string, object>{
                    { "health_check.name", nameof(CloudFirestoreHealthCheck) },
                    { "health_check.task", "ready" },
                    { "db.system.name", "gcpcloudfirestore" }
    };

    public CloudFirestoreHealthCheck(CloudFirestoreOptions cloudFirestoreOptions)
    {
        _cloudFirestoreOptions = Guard.ThrowIfNull(cloudFirestoreOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> checkDetails = _baseCheckDetails;
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
                        data: new ReadOnlyDictionary<string, object>(checkDetails)
                    );
                }
            }

            return HealthCheckResult.Healthy(data: new ReadOnlyDictionary<string, object>(checkDetails));
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex, data: new ReadOnlyDictionary<string, object>(checkDetails));
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
