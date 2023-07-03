namespace HealthChecks.AzureSearch;

/// <summary>
/// Options for <see cref="AzureSearchHealthCheck"/>.
/// </summary>
public class AzureSearchOptions
{
    /// <summary>
    /// The URI endpoint of the Azure Search
    /// </summary>
    public string Endpoint { get; set; } = null!;

    /// <summary>
    /// The name of the Search index
    /// </summary>
    public string IndexName { get; set; } = null!;

    /// <summary>
    /// The API credential used to authenticate against the Search service
    /// </summary>
    public string AuthKey { get; set; } = null!;
}
