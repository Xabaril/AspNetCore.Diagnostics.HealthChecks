using HealthChecks.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.DependencyInjection.SignalR
{
    public class signalr_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("signalr", typeof(SignalRHealthCheck), builder => builder.AddSignalRHub("https://signalr.com/echo"));
        }
    }
}
