using HealthChecks.Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.Hangfire
{
    public class hangfire_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("hangfire", typeof(HangfireHealthCheck), builder => builder.AddHangfire(
                setup => setup.MaximumJobsFailed = 3));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("my-hangfire-group", typeof(HangfireHealthCheck), builder => builder.AddHangfire(
                setup => setup.MaximumJobsFailed = 3, name: "my-hangfire-group"));
        }
    }
}
