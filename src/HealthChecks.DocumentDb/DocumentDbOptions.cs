namespace HealthChecks.DocumentDb;

/// <summary>
/// Options for <see cref="DocumentDbHealthCheck"/>.
/// </summary>
public class DocumentDbOptions
{
    public string UriEndpoint { get; set; } = null!;

    public string PrimaryKey { get; set; } = null!;

    public string? DatabaseName { get; set; }

    public string? CollectionName { get; set; }
}
