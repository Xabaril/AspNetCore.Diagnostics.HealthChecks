using Microsoft.AspNetCore.Builder;

namespace HealthChecks.UI;

internal class HealthCheckUIConventionBuilder : IEndpointConventionBuilder
{
    private readonly IEnumerable<IEndpointConventionBuilder> _endpoints;

    public HealthCheckUIConventionBuilder(IEnumerable<IEndpointConventionBuilder> endpoints)
    {
        _endpoints = Guard.ThrowIfNull(endpoints);
    }

    public void Add(Action<EndpointBuilder> convention)
    {
        foreach (var endpoint in _endpoints)
        {
            endpoint.Add(convention);
        }
    }
}
