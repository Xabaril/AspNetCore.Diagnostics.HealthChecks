namespace HealthChecks.AzureSearch;

/// <summary>
/// Options for <see cref="AzureSearchHealthCheck"/>.
/// </summary>
public class AzureSearchOptions
{
    public string Endpoint { get; set; } = null!;

    public string IndexName { get; set; } = null!;

    public string AuthKey { get; set; } = null!;
}
