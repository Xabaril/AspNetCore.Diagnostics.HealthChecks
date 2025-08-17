namespace HealthChecks.UI.Tests;

public class ProviderTestHelper
{
    public const int DefaultHostTimeout = 1000;
    public const int DefaultCollectorTimeout = 10000;

    public static List<(string Name, string Uri)> Endpoints = new()
    {
        ("host1", "http://localhost/health"),
        ("host2", "http://localhost/health")
    };
}
