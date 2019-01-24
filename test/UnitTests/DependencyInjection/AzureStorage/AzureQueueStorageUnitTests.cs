using FluentAssertions;
using HealthChecks.AzureStorage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.DependencyInjection.AzureStorage
{
    public class azurequeuestorage_registration_should
    {
        [Fact]
        public void not_throw_when_connectionstring_is_invalid()
        {
            var connectionString = "bla";
            var services = new ServiceCollection();
            services.AddHealthChecks()
                    .AddAzureQueueStorage(connectionString);

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurequeue");
            check.GetType().Should().Be(typeof(AzureQueueStorageHealthCheck));
        }
    }
}
