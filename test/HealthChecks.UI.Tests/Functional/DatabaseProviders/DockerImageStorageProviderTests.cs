using HealthChecks.UI.Data;
using HealthChecks.UI.Image;
using HealthChecks.UI.Image.Configuration;
using HealthChecks.UI.Tests.Fixtures;
using Microsoft.Extensions.Configuration;

namespace HealthChecks.UI.Tests;

public class docker_image_storage_provider_configuration_should
{
    private const string SqlProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string PostgreProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";

    [Fact]
    public void fail_with_invalid_storage_provider_value()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", "invalidvalue")
                });
            })
            .UseStartup<Startup>();

        Should.Throw<ArgumentException>(() => hostBuilder.Build());
    }
    [Fact]
    public void register_sql_server()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.SqlServer.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
                });
            })
            .UseStartup<Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqlProviderName);
    }

    [Fact]
    public void fail_to_register_sql_server_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.SqlServer.ToString())
                });
            })
            .UseStartup<Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_sqlite()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.Sqlite.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
                });
            })
            .UseStartup<Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqliteProviderName);
    }

    [Fact]
    public void fail_to_register_sqlite_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.Sqlite.ToString())
                });
            })
            .UseStartup<Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_postgresql()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.PostgreSql.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", "connectionstring"),
                });
            })
            .UseStartup<Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(PostgreProviderName);
    }

    [Fact]
    public void fail_to_register_postgresql_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.PostgreSql.ToString())
                });
            })
            .UseStartup<Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_inmemory()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.InMemory.ToString())
                });
            })
            .UseStartup<Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }

    [Fact]
    public void register_inmemory_as_default_provider_when_no_option_is_configured()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config => config.Sources.Clear())
            .UseStartup<Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }
}

[Collection("execution")]
public class docker_image_storage_provider_mysql_configuration_should(MySqlContainerFixture mySqlFixture) : IClassFixture<MySqlContainerFixture>
{
    private const string MySqlProviderName = "Pomelo.EntityFrameworkCore.MySql";

    [Fact]
    public void register_mysql()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.MySql.ToString()),
                    new KeyValuePair<string, string?>("storage_connection", mySqlFixture.GetConnectionString()),
                });
            })
            .UseStartup<Startup>();

        var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(MySqlProviderName);
    }

    [Fact]
    public void fail_to_register_mysql_with_no_connection_string()
    {
        var hostBuilder = new WebHostBuilder()
            .ConfigureAppConfiguration(config =>
            {
                config.Sources.Clear();

                config.AddInMemoryCollection(new List<KeyValuePair<string, string?>>
                {
                    new KeyValuePair<string, string?>("storage_provider", StorageProviderEnum.MySql.ToString())
                });
            })
            .UseStartup<Startup>();

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }
}
