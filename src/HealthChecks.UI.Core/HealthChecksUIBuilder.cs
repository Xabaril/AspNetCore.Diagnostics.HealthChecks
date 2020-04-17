using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class HealthChecksUIBuilder
    {
        public IServiceCollection Services { get; }
        public HealthChecksUIBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
}
