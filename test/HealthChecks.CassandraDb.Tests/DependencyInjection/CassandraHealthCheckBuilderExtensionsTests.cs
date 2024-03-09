using HealthChecks.CassandraDb.DependencyInjection;

namespace HealthChecks.CassandraDb.Tests.DependencyInjection;

public class CassandraHealthCheckBuilderExtensionsTests
{
    [Fact]
    public void AddHealthCheckWhenProperlyConfigured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddCassandra(new CassandraDbOptions
            {
                ContactPoint = "cassandradb",
                Keyspace = "myKeyspace",
                Query = "SELECT now() FROM system.local;",
                ConfigureClusterBuilder = builder => builder.WithPort(9042)
            }, name: "cassandra")
            .Services;

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
        var healthCheckRegistration = options?.Value.Registrations.First();
        var registration = healthCheckRegistration;

        registration?.Name.ShouldBe("cassandra");
    }

    [Fact]
    public void AddNamedHealthCheckWhenProperlyConfigured()
    {
        var services = new ServiceCollection()
            .AddHealthChecks()
            .AddCassandra(new CassandraDbOptions
            {
                ContactPoint = "cassandradb",
                Keyspace = "testKeyspace",
                Query = "SELECT now() FROM system.local;",
                ConfigureClusterBuilder = builder => builder.WithPort(9042)
            }, name: "cassandra")
            .Services;

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetService<IOptions<HealthCheckServiceOptions>>();
        var registration = options?.Value.Registrations.First(r => r.Name == "my-cassandra-1");

        registration?.Name.ShouldBe("cassandra");
    }

    [Fact]
    public void ThrowsExceptionWhenConfigureClusterBuilderIsNullAndRequired()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() => services.AddHealthChecks().AddCassandra(new CassandraDbOptions
        {
            ContactPoint = "cassandradb",
            Keyspace = "myKeyspace",
        }));
    }
}
