using HealthChecks.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace UnitTests.HealthChecks.DependencyInjection.AzureKeyVault
{
    public class azure_keyvault_registration_should : base_should
    {
        [Fact]
        public void add_health_check_when_properly_configured()
        {
            ShouldPass("azurekeyvault", typeof(AzureKeyVaultHealthCheck), builder => builder.AddAzureKeyVault(setup =>
            {
                setup
                .UseKeyVaultUrl("https://keyvault")
                .AddSecret("supercret");
            }));
        }
        [Fact]
        public void add_named_health_check_when_properly_configured()
        {
            ShouldPass("keyvaultcheck", typeof(AzureKeyVaultHealthCheck), builder => builder.AddAzureKeyVault(setup =>
            {
                setup
                .UseKeyVaultUrl("https://keyvault")
                .UseClientSecrets("client", "secret");
            }, name: "keyvaultcheck"));
        }
        [Fact]
        public void fail_when_invalidad_uri_provided_in_configuration()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() =>
            {
                services.AddHealthChecks()
                .AddAzureKeyVault(setup =>
                {
                    setup
                    .UseKeyVaultUrl("invalid URI")
                    .AddSecret("mysecret");
                });
            });
        }
    }
}
