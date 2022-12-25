namespace Microsoft.Extensions.DependencyInjection
{
    public class HealthChecksUIBuilder
    {
        public HealthChecksUIBuilder(IServiceCollection services)
        {
            Services = Guard.ThrowIfNull(services);
        }

        public IServiceCollection Services { get; }
    }
}
