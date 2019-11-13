using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthChecks.UI
{
    class HealthCheckUIConventionBuilder : IEndpointConventionBuilder
    {
        private readonly IEndpointConventionBuilder ApiEndpoint;
        private readonly IEndpointConventionBuilder WebhooksEndpoint;

        public HealthCheckUIConventionBuilder(IEndpointConventionBuilder apiEndpoint, IEndpointConventionBuilder webhooksEndpoint)
        {
            ApiEndpoint = apiEndpoint ?? throw new ArgumentNullException(nameof(apiEndpoint));
            WebhooksEndpoint = webhooksEndpoint ?? throw new ArgumentNullException(nameof(webhooksEndpoint));
        }

        public void Add(Action<EndpointBuilder> convention)
        {
            ApiEndpoint.Add(convention);
            WebhooksEndpoint.Add(convention);
        }
    }
}
