namespace HealthChecks.CosmosDb;

/// <summary>
/// Represents a collection of settings that configure an
/// <see cref="AzureCosmosDbHealthCheck">Azure Cosmos DB health check</see>.
/// </summary>
public sealed class AzureCosmosDbHealthCheckOptions
{
    // TODO: Consider implementing IValidatableObject to ensure ContainerIds are not specified
    //       when DatabaseId is missing. Furthermore, it may be advantageous to use IReadOnlyCollection<T>
    //       instead of IEnumerable<T> for the use of the Count property.

    /// <summary>
    /// Gets or sets zero or more identifiers for the Azure Cosmos DB containers
    /// within the database with the specified <see cref="DatabaseId"/> whose health should be checked.
    /// </summary>
    /// <remarks>
    /// If the value is <see langword="null"/>, then no health check is performed for containers.
    /// </remarks>
    /// <value>Zero or more Azure Cosmos DB container identifiers.</value>
    public IEnumerable<string>? ContainerIds { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the Azure Cosmos database whose health should be checked.
    /// </summary>
    /// <remarks>
    /// If the value is <see langword="null"/>, then no health check is performed for a specific database.
    /// </remarks>
    /// <value>An optional Azure Cosmos database identifier.</value>
    public string? DatabaseId { get; set; }
}
