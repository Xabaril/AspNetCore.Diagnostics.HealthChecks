using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ConsoleApp1;

/// <summary>
/// This program tests issue of loading Microsoft.Bcl.AsyncInterfaces assembly on NET6.
/// See https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/1975
/// </summary>
internal class Program
{
    private static async Task Main()
    {
        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddHealthChecks()
            .AddAzureBlobStorage("BlobEndpoint=https://unit-test.blob.core.windows.net")
            .Services
            .BuildServiceProvider();

        var service = serviceProvider.GetRequiredService<HealthCheckService>();
        var report = await service.CheckHealthAsync().ConfigureAwait(false);
        Console.WriteLine("Hello, World!");
    }
}
