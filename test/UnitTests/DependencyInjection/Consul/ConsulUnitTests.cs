using HealthChecks.Consul;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.DependencyInjection.Consul
{
    public class ConsulUnitTests : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("consul", typeof(ConsulHealthCheck), builder => builder.AddConsul(setup =>
            {
                setup.HostName = "hostname";
                setup.Port = 8500;
                setup.RequireHttps = false;
            }));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-consul-group", typeof(ConsulHealthCheck), builder => builder.AddConsul(setup =>
            {
                setup.HostName = "hostname";
                setup.Port = 8500;
                setup.RequireHttps = false;
            }, name: "my-consul-group"));
        }
    }
}