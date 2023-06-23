using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.AzureSearch.DependencyInjection;

public static class AzureSearchHealthCheckBuilderExtensions
{
    private const string NAME = "azuresearch";

    public static IHealthChecksBuilder AddAzureSearch(
        this IHealthChecksBuilder builder,
        Action<AzureSearchOptions>? setup,
        string? name = default,
        HealthStatus? failureStatus = default,
        IEnumerable<string>? tags = default,
        TimeSpan? timeout = default)
    {
        var azureSearchOptions = new AzureSearchOptions();
        setup?.Invoke(azureSearchOptions);

        return builder.Add(new HealthCheckRegistration(
           name ?? NAME,
           sp => new AzureSearchHealthCheck(azureSearchOptions),
           failureStatus,
           tags,
           timeout));
    }
}
