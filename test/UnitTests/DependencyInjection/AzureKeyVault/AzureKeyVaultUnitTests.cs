using Azure.Core;
using FluentAssertions;
using HealthChecks.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                .AddAzureKeyVault(new Uri("http://localhost"), new MockTokenCredentials(), setup =>
                 {
                     setup
                     .AddSecret("supersecret")
                     .AddKey("mycryptokey");
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
                .AddAzureKeyVault(new Uri("http://localhost"), new MockTokenCredentials(),options=> { }, name: "keyvaultcheck");

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("keyvaultcheck");
            check.GetType().Should().Be(typeof(AzureKeyVaultHealthCheck));
        }

        [Fact]
        public void fail_when_invalid_uri_provided_in_configuration()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() =>
            {
                services.AddHealthChecks()
                .AddAzureKeyVault(null, new MockTokenCredentials(), setup =>
                 {
                     setup
                     .AddSecret("mysecret")
                     .AddKey("mycryptokey");
                 });
            });
        }

        [Fact]
        public void fail_when_invalid_uri_provided_with_service_provider_based_setup_in_configuration()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() =>
            {
                services.AddHealthChecks()
                .AddAzureKeyVault(null as Uri, new MockTokenCredentials(), (_, setup) =>
                {
                    setup
                    .AddSecret("mysecret")
                    .AddKey("mycryptokey");
                });
            });
        }

        [Fact]
        public void fail_when_invalid_credential_provided_in_configuration()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentNullException>(() =>
            {
                services.AddHealthChecks()
                    .AddAzureKeyVault(new Uri("http://localhost"), null, setup =>
                    {
                        setup
                            .AddSecret("mysecret")
                            .AddKey("mycryptokey");
                    });
            });
        }

        [Fact]
        public void add_health_check_with_keyvault_service_uri_factory_when_properly_configured()
        {
            var services = new ServiceCollection();
            var factoryCalled = false;
            var setupCalled = false;

            services.AddHealthChecks()
                .AddAzureKeyVault(
                    _ =>
                    {
                        factoryCalled = true;
                        return new Uri("http://localhost");
                    }, 
                    new MockTokenCredentials(), 
                    (_, _) => { setupCalled = true; }
                );

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            check.GetType().Should().Be(typeof(AzureKeyVaultHealthCheck));
            factoryCalled.Should().BeTrue();
            setupCalled.Should().BeTrue();
        }

        [Fact]
        public void add_health_check_with_service_provider_based_setup_when_properly_configured()
        {
            var services = new ServiceCollection();
            var setupCalled = false;

            services.AddHealthChecks()
                .AddAzureKeyVault(
                    new Uri("http://localhost"),
                    new MockTokenCredentials(),
                    (_, _) => { setupCalled = true; }
                );

            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            check.GetType().Should().Be(typeof(AzureKeyVaultHealthCheck));
            setupCalled.Should().BeTrue();
        }
    }

    public class MockTokenCredentials : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}