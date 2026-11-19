using HealthChecks.UI.Data;
using HealthChecks.UI.Image;
using HealthChecks.UI.Image.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HealthChecks.UI.Tests;

public class docker_image_storage_provider_configuration_should
{
    private const string SqlProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
    private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
    private const string PostgreProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
    private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";
    // MySQL depolaması Pomelo'dan Oracle'ın MySql.EntityFrameworkCore paketine geçirildi (net10 yükseltmesi).
    private const string MySqlProviderName = "MySql.EntityFrameworkCore";

    // Docker imajının Startup sınıfı yalnız UseStartup ile sınanabilir. WebApplicationBuilder.WebHost.UseStartup
    // çalışma zamanında NotSupportedException fırlattığı için (ASP0010) eski WebHostBuilder kalıbının genel host
    // karşılığı kullanılır. Kaynaklar temizlenir: imaj yalnız verilen ayarları görür.
    private static IHostBuilder CreateHostBuilder(params (string Key, string Value)[] settings) =>
        new HostBuilder()
            .ConfigureWebHost(webHost => webHost
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(settings.Select(s => new KeyValuePair<string, string?>(s.Key, s.Value)));
                })
                .UseStartup<Startup>());

    [Fact]
    public void fail_with_invalid_storage_provider_value()
    {
        var hostBuilder = CreateHostBuilder(("storage_provider", "invalidvalue"));

        Should.Throw<ArgumentException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_sql_server()
    {
        var hostBuilder = CreateHostBuilder(
            ("storage_provider", StorageProviderEnum.SqlServer.ToString()),
            ("storage_connection", "connectionstring"));

        using var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqlProviderName);
    }

    [Fact]
    public void fail_to_register_sql_server_with_no_connection_string()
    {
        var hostBuilder = CreateHostBuilder(("storage_provider", StorageProviderEnum.SqlServer.ToString()));

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_sqlite()
    {
        var hostBuilder = CreateHostBuilder(
            ("storage_provider", StorageProviderEnum.Sqlite.ToString()),
            ("storage_connection", "connectionstring"));

        using var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(SqliteProviderName);
    }

    [Fact]
    public void fail_to_register_sqlite_with_no_connection_string()
    {
        var hostBuilder = CreateHostBuilder(("storage_provider", StorageProviderEnum.Sqlite.ToString()));

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_postgresql()
    {
        var hostBuilder = CreateHostBuilder(
            ("storage_provider", StorageProviderEnum.PostgreSql.ToString()),
            ("storage_connection", "connectionstring"));

        using var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(PostgreProviderName);
    }

    [Fact]
    public void fail_to_register_postgresql_with_no_connection_string()
    {
        var hostBuilder = CreateHostBuilder(("storage_provider", StorageProviderEnum.PostgreSql.ToString()));

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }

    [Fact]
    public void register_inmemory()
    {
        var hostBuilder = CreateHostBuilder(("storage_provider", StorageProviderEnum.InMemory.ToString()));

        using var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }

    [Fact]
    public void register_inmemory_as_default_provider_when_no_option_is_configured()
    {
        var hostBuilder = CreateHostBuilder();

        using var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(InMemoryProviderName);
    }

    [Fact]
    public void register_mysql()
    {
        var hostBuilder = CreateHostBuilder(
            ("storage_provider", StorageProviderEnum.MySql.ToString()),
            ("storage_connection", "Host=localhost;User Id=root;Password=Password12!;Database=UI"));

        using var host = hostBuilder.Build();

        var context = host.Services.GetRequiredService<HealthChecksDb>();
        context.Database.ProviderName.ShouldBe(MySqlProviderName);
    }

    [Fact]
    public void fail_to_register_mysql_with_no_connection_string()
    {
        var hostBuilder = CreateHostBuilder(("storage_provider", StorageProviderEnum.MySql.ToString()));

        Should.Throw<ArgumentNullException>(() => hostBuilder.Build());
    }
}
