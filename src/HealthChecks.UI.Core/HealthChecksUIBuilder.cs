namespace Microsoft.Extensions.DependencyInjection
{
    public class HealthChecksUIBuilder
    {
        public IServiceCollection Services { get; }
        public HealthChecksUIBuilder(IServiceCollection services)
        {
            Services = Guard.ThrowIfNull(services);
        }
    }
}
