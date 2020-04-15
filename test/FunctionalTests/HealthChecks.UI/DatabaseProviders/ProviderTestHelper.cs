using FunctionalTests.Base;
using System.Collections.Generic;

namespace FunctionalTests.HealthChecks.UI.DatabaseProviders
{
    public class ProviderTestHelper
    {
        public const int DefaultHostTimeout = 1000;
        public const int DefaultCollectorTimeout = 10000;

        public static List<(string Name, string Uri)> Endpoints = new List<(string Name, string Uri)>
        {
            ("host1", "http://localhost/health"),
            ("host2", "http://localhost/health")
        };

        public static string SqlServerConnectionString(ExecutionFixture fixture) =>
            fixture.IsAppVeyorExecution
                ? @"Server=(local)\SQL2017;Initial Catalog=UI;User Id=sa;Password=Password12!"
                : "Server=tcp:127.0.0.1,1833;Initial Catalog=UI;User Id=sa;Password=Password12!";
        public static string PostgresConnectionString(ExecutionFixture fixture) =>
             fixture.IsAppVeyorExecution ?
                 "Server=127.0.0.1;Port=5432;User ID=postgres;Password=Password12!;database=ui" :
                 "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=ui";

        public static string MySqlConnectionString(ExecutionFixture fixture) =>
                      fixture.IsAppVeyorExecution ?
                 "Host=localhost;User Id=root;Password=Password12!;Database=UI" :
                 "Host=localhost;User Id=root;Password=Password12!;Database=UI";

        public static string SqliteConnectionString() => "Data Source = sqlite.db";
    }
}
