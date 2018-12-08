using FluentAssertions;
using HealthChecks.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureKeyVault
{
    public class azure_keyvault_registration_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureKeyVault(setup =>
                {
                    setup.KeyVaultUrlBase = "http://url";
                    setup.ClientId = "value";
                    setup.ClientSecret = "value";
                });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azurekeyvault");
            check.GetType().Should().Be(typeof(AzureKeyVaultHealthCheck));

        }

        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureKeyVault(setup =>
                {
                    setup.KeyVaultUrlBase = "http://url";
                    setup.ClientId = "value";
                    setup.ClientSecret = "value";
                }, name: "keyvaultcheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("keyvaultcheck");
            check.GetType().Should().Be(typeof(AzureKeyVaultHealthCheck));
        }

        [Fact]
        public void fail_when_no_health_check_configuration_provided()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureKeyVault(setup => { });

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();

            Assert.Throws<ArgumentNullException>(() => registration.Factory(serviceProvider));
        }
    }
}
