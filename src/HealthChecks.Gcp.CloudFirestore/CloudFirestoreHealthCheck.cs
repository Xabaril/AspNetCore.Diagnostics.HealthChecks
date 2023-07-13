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
        try
        {
            var currentRootCollections = await GetRootCollectionsAsync(cancellationToken).ConfigureAwait(false);
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
            cancellationToken.ThrowIfCancellationRequested();
            collections.Add(item.Id);
        }

        return collections;
    }
}
