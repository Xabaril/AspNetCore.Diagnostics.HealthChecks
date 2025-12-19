using System.Collections.Concurrent;
using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureSearch;

/// <summary>
/// Represents a health check, which can be used to check the status of Azure Search.
/// </summary>
public class AzureSearchHealthCheck : IHealthCheck
{
    private readonly ConcurrentDictionary<string, SearchClient> _connections = new();
    private readonly AzureSearchOptions _searchOptions = new();

    public AzureSearchHealthCheck(AzureSearchOptions searchOptions)
    {
        _searchOptions.Endpoint = Guard.ThrowIfNull(searchOptions.Endpoint);
        _searchOptions.IndexName = Guard.ThrowIfNull(searchOptions.IndexName);

        if (searchOptions.AuthKey is null && searchOptions.TokenCredential is null)
        {
            throw new ArgumentException($"Either {nameof(_searchOptions.AuthKey)} or {nameof(_searchOptions.TokenCredential)} must be set");
        }

        _searchOptions.TokenCredential = searchOptions.TokenCredential;
        _searchOptions.AuthKey = searchOptions.AuthKey;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var searchClient = _connections.GetOrAdd(GetCacheKey(), CreateSearchClient());

            _ = await searchClient.GetDocumentCountAsync(cancellationToken).ConfigureAwait(false);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private SearchClient CreateSearchClient()
    {
        var uri = new Uri(_searchOptions.Endpoint);
        return _searchOptions.TokenCredential is not null
            ? new SearchClient(uri, _searchOptions.IndexName, _searchOptions.TokenCredential!)
            : new SearchClient(uri, _searchOptions.IndexName, new AzureKeyCredential(_searchOptions.AuthKey!));
    }

    private string GetCacheKey() => $"{_searchOptions.Endpoint}_{_searchOptions.IndexName}";
}
