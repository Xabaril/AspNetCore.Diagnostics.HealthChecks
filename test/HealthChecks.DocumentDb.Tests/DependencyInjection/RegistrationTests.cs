using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System.Linq;
using Xunit;

namespace HealthChecks.DocumentDb.Tests.DependencyInjection
{
    public class documentdb_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDocumentDb(_ => { _.PrimaryKey = "key"; _.UriEndpoint = "endpoint"; });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("documentdb");
            check.GetType().Should().Be(typeof(DocumentDbHealthCheck));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddDocumentDb(_ => { _.PrimaryKey = "key"; _.UriEndpoint = "endpoint"; }, name: "my-documentdb-group");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-documentdb-group");
            check.GetType().Should().Be(typeof(DocumentDbHealthCheck));
        }
    }
}
