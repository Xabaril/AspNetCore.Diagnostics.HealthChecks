using HealthChecks.UI.Core.Data;
using HealthChecks.UI.Image;
using HealthChecks.UI.Image.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.UI.DatabaseProviders
{
    public class docker_image_storage_provider_configuration_should
    {
        private const string SqlProviderName = "Microsoft.EntityFrameworkCore.SqlServer";
        private const string SqliteProviderName = "Microsoft.EntityFrameworkCore.Sqlite";
        private const string PostgreProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
        private const string InMemoryProviderName = "Microsoft.EntityFrameworkCore.InMemory";
        private const string MySqlProviderName = "Pomelo.EntityFrameworkCore.MySql";

        [Fact]
        public void fail_with_invalid_storage_provider_value()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", "invalidvalue")
                    });
                })
                .UseStartup<Startup>();

            Assert.Throws<ArgumentException>(() => hostBuilder.Build());
        }
        [Fact]
        public void register_sql_server()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.SqlServer.ToString()),
                        new KeyValuePair<string,string>("storage_connection", "connectionstring"),
                    });
                })
                .UseStartup<Startup>();

            var host = hostBuilder.Build();

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            context.Database.ProviderName.Equals(SqlProviderName);
        }

        [Fact]
        public void fail_to_register_sql_server_with_no_connection_string()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.SqlServer.ToString())
                    });
                })
                .UseStartup<Startup>();

            Assert.Throws<ArgumentNullException>(() => hostBuilder.Build());
        }

        [Fact]
        public void register_sqlite()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.Sqlite.ToString()),
                        new KeyValuePair<string,string>("storage_connection", "connectionstring"),
                    });
                })
                .UseStartup<Startup>();

            var host = hostBuilder.Build();

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            context.Database.ProviderName.Equals(SqliteProviderName);
        }

        [Fact]
        public void fail_to_register_sqlite_with_no_connection_string()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.Sqlite.ToString())
                    });
                })
                .UseStartup<Startup>();

            Assert.Throws<ArgumentNullException>(() => hostBuilder.Build());
        }

        [Fact]
        public void register_postgresql()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.PostgreSql.ToString()),
                        new KeyValuePair<string,string>("storage_connection", "connectionstring"),
                    });
                })
                .UseStartup<Startup>();

            var host = hostBuilder.Build();

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            context.Database.ProviderName.Equals(PostgreProviderName);
        }

        [Fact]
        public void fail_to_register_postgresql_with_no_connection_string()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.PostgreSql.ToString())
                    });
                })
                .UseStartup<Startup>();

            Assert.Throws<ArgumentNullException>(() => hostBuilder.Build());
        }

        [Fact]
        public void register_inmemory()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.InMemory.ToString())
                    });
                })
                .UseStartup<Startup>();

            var host = hostBuilder.Build();

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            context.Database.ProviderName.Equals(InMemoryProviderName);
        }

        [Fact]
        public void register_inmemory_as_default_provider_when_no_option_is_configured()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                })
                .UseStartup<Startup>();

            var host = hostBuilder.Build();

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            context.Database.ProviderName.Equals(InMemoryProviderName);
        }

        [Fact]
        public void register_mysql()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.MySql.ToString()),
                        new KeyValuePair<string,string>("storage_connection", "Host=localhost;User Id=root;Password=Password12!;Database=UI"),

                    });
                })
                .UseStartup<Startup>();

            var host = hostBuilder.Build();

            var context = host.Services.GetRequiredService<HealthChecksDb>();
            context.Database.ProviderName.Equals(MySqlProviderName);
        }

        [Fact]
        public void fail_to_register_mysql_with_no_connection_string()
        {
            var hostBuilder = new WebHostBuilder()
                .ConfigureAppConfiguration(config =>
                {
                    config.Sources.Clear();

                    config.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string,string>("storage_provider", StorageProviderEnum.MySql.ToString())
                    });
                })
                .UseStartup<Startup>();

            Assert.Throws<ArgumentNullException>(() => hostBuilder.Build());
        }

    }
}
