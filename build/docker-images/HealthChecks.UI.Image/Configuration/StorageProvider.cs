using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace HealthChecks.UI.Image.Configuration
{
    public class Storage
    {
        public static IDictionary<StorageProviderEnum, StorageProvider> GetProviders()
        {
            return new Dictionary<StorageProviderEnum, StorageProvider>
            {
                [StorageProviderEnum.InMemory] = new StorageProvider
                {
                    SetupProvider = (builder, connection) => builder.AddInMemoryStorage(),
                    RequiresConnectionString = false
                },
                [StorageProviderEnum.SqlServer] = new StorageProvider
                {
                    SetupProvider = (builder, connection) => builder.AddSqlServerStorage(connection),
                    RequiresConnectionString = true,
                },
                [StorageProviderEnum.Sqlite] = new StorageProvider
                {
                    SetupProvider = (builder, connection) => builder.AddSqliteStorage(connection),
                    RequiresConnectionString = true,
                },
                [StorageProviderEnum.PostgreSql] = new StorageProvider
                {
                    SetupProvider = (builder, connection) => builder.AddPostgreSqlStorage(connection),
                    RequiresConnectionString = true,
                },
                [StorageProviderEnum.MySql] = new StorageProvider
                {
                    SetupProvider = (builder, connection) => builder.AddMySqlStorage(connection),
                    RequiresConnectionString = true,
                }
            };
        }
    }

    public class StorageProvider
    {
        public bool RequiresConnectionString { get; set; }
        public Action<HealthChecksUIBuilder, string> SetupProvider { get; set; }
    }

    public enum StorageProviderEnum
    {
        InMemory = 0,
        SqlServer = 1,
        Sqlite = 2,
        PostgreSql = 3,
        MySql = 4
    }
}
