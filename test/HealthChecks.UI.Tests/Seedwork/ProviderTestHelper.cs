namespace HealthChecks.UI.Tests
{
    public class ProviderTestHelper
    {
        public const int DefaultHostTimeout = 1000;
        public const int DefaultCollectorTimeout = 10000;

        public static List<(string Name, string Uri)> Endpoints = new()
        {
            ("host1", "http://localhost/health"),
            ("host2", "http://localhost/health")
        };

        public static string SqlServerConnectionString() => "Server=tcp:localhost,5433;Initial Catalog=master;User Id=sa;Password=Password12!;TrustServerCertificate=true";
        public static string PostgresConnectionString() => "Server=127.0.0.1;Port=8010;User ID=postgres;Password=Password12!;database=ui";
        public static string MySqlConnectionString() => "Host=localhost;User Id=root;Password=Password12!;Database=UI";
        public static string SqliteConnectionString() => "Data Source = sqlite.db";
    }
}
