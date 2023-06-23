using System.Collections.Concurrent;
using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureSearch;

public class AzureSearchHealthCheck : IHealthCheck
{
    private readonly ConcurrentDictionary<string, SearchClient> _connections = new();
    private readonly AzureSearchOptions _searchOptions = new();

    public AzureSearchHealthCheck(AzureSearchOptions searchOptions)
    {
        _searchOptions.Endpoint = searchOptions.Endpoint;
        _searchOptions.IndexName = searchOptions.IndexName;
        _searchOptions.AuthKey = searchOptions.AuthKey;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"{_searchOptions.Endpoint}_{_searchOptions.IndexName}";

            var searchClient = _connections.GetOrAdd(cacheKey, CreateSearchClient());

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
        var credential = new AzureKeyCredential(_searchOptions.AuthKey);

        return new SearchClient(uri, _searchOptions.IndexName, credential);
    }
}
