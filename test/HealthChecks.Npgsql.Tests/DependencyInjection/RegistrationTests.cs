using System.Reflection;
using HealthChecks.NpgSql;
using Npgsql;

namespace HealthChecks.Npgsql.Tests.DependencyInjection;

public class npgsql_registration_should
{
    [Fact]
    public void add_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNpgSql("Server=localhost");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("npgsql");
        check.ShouldBeOfType<NpgSqlHealthCheck>();

    }

    [Fact]
    public void add_named_health_check_when_properly_configured()
    {
        var services = new ServiceCollection();
        services.AddHealthChecks()
            .AddNpgSql("Server=localhost", name: "my-npg-1");

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-npg-1");
        check.ShouldBeOfType<NpgSqlHealthCheck>();
    }

    [Fact]
    public void add_health_check_with_connection_string_factory_when_properly_configured()
    {
        var services = new ServiceCollection();
        var factoryCalled = false;
        services.AddHealthChecks()
            .AddNpgSql(_ =>
            {
                factoryCalled = true;
                return "Server=localhost";
            }, name: "my-npg-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.First();
        var check = registration.Factory(serviceProvider);

        registration.Name.ShouldBe("my-npg-1");
        check.ShouldBeOfType<NpgSqlHealthCheck>();
        factoryCalled.ShouldBeTrue();
    }

    [Fact]
    public void factory_is_called_only_once()
    {
        ServiceCollection services = new();
        int factoryCalls = 0;
        services.AddHealthChecks()
            .AddNpgSql(_ =>
            {
                Interlocked.Increment(ref factoryCalls);
                return "Server=localhost";
            }, name: "my-npg-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.Single();

        for (int i = 0; i < 10; i++)
        {
            _ = registration.Factory(serviceProvider);
        }

        factoryCalls.ShouldBe(1);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void factory_reuses_pre_registered_datasource_when_possible(bool sameConnectionString)
    {
        const string connectionString = "Host=localhost";
        ServiceCollection services = new();

        services.AddSingleton<NpgsqlDataSource>(serviceProvider =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            return dataSourceBuilder.Build();
        });

        int factoryCalls = 0;
        services.AddHealthChecks()
            .AddNpgSql(_ =>
            {
                Interlocked.Increment(ref factoryCalls);
                return sameConnectionString ? connectionString : $"{connectionString}2";
            }, name: "my-npg-1");

        using var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();

        var registration = options.Value.Registrations.Single();

        for (int i = 0; i < 10; i++)
        {
            var healthCheck = (NpgSqlHealthCheck)registration.Factory(serviceProvider);
            var fieldInfo = typeof(NpgSqlHealthCheck).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic);
            var npgSqlHealthCheckOptions = (NpgSqlHealthCheckOptions)fieldInfo!.GetValue(healthCheck)!;

            Assert.Equal(sameConnectionString, ReferenceEquals(serviceProvider.GetRequiredService<NpgsqlDataSource>(), npgSqlHealthCheckOptions.DataSource));
        }

        factoryCalls.ShouldBe(1);
    }

    [Fact]
    public void recommended_scenario_compiles_and_works_as_expected()
    {
        ServiceCollection services = new();
        services.AddNpgsqlDataSource("Host=pg_server;Username=test;Password=test;Database=test");
        services.AddHealthChecks().AddNpgSql();

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
        var registration = options.Value.Registrations.Single();
        var healthCheck = registration.Factory(serviceProvider);

        healthCheck.ShouldBeOfType<NpgSqlHealthCheck>();
    }
}
