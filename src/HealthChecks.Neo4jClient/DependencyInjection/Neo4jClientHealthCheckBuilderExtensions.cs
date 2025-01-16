namespace Microsoft.Extensions.DependencyInjection;

public static class Neo4jClientHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddNeo4jClient(this IHealthChecksBuilder builder)
    {

        return builder;
    }
}
