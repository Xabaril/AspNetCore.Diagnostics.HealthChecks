using Azure.Core;
using Azure.Data.Tables;

namespace HealthChecks.CosmosDb.Tests.DependencyInjection
{
    public class cosmosdb_registration_should
    {
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("myconnectionstring");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured_using_token_credential()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("cosmosdbaccounturi", new MockTokenCredential());

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured_with_database()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("myconnectionstring", "dabasename");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured_using_token_credential_with_database()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("cosmosdbaccounturi", new MockTokenCredential(), "databasename");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured_with_database_and_collections()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDbCollection("myconnectionstring", "dabasename", collections: new[] { "first-collection", "second_collections" });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_health_check_when_properly_configured_using_token_credential_with_database_and_collections()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDbCollection("cosmosdbaccounturi", new MockTokenCredential(), "dabasename", collections: new[] { "first-collection", "second_collections" });

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("cosmosdb");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }
        [Fact]
        public void add_cosmosdb_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddCosmosDb("myconnectionstring", name: "my-cosmosdb-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-cosmosdb-group");
            check.GetType().Should().Be(typeof(CosmosDbHealthCheck));
        }

        [Fact]
        public void add_azuretable_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable("myconnectionstring", "tableName");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
        [Fact]
        public void add_azuretable_health_check_when_properly_configured_with_database()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable("myconnectionstring", "tableName");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
        [Fact]
        public void add_azuretable_health_check_when_properly_configured_using_table_shared_key_credential()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable(new Uri("http://localhost"), new TableSharedKeyCredential("mystorageaccount", "dGVzdEtleQ=="), "tableName");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
        [Fact]
        public void add_azuretable_health_check_when_properly_configured_using_token_credential()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable(new Uri("http://localhost"), new MockTokenCredential(), "tableName");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("azuretable");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
        [Fact]
        public void add_azuretable_named_health_check_when_properly_configured()
        {
            var services = new ServiceCollection();
            services.AddHealthChecks()
                .AddAzureTable("myconnectionstring", "tableName", name: "my-azuretable-group");

            using var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

            var registration = options.Value.Registrations.First();
            var check = registration.Factory(serviceProvider);

            registration.Name.Should().Be("my-azuretable-group");
            check.GetType().Should().Be(typeof(TableServiceHealthCheck));
        }
    }

    public class MockTokenCredential : TokenCredential
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
