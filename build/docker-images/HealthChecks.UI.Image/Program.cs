using HealthChecks.UI.Image.Configuration;
using HealthChecks.UI.Image.Extensions;

namespace HealthChecks.UI.Image;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .ConfigureLogging(config => config.AddFilter(typeof(Program).Namespace, LogLevel.Information))
                .ConfigureAppConfiguration((context, builder) =>
                {
                    if (AzureAppConfiguration.Enabled)
                    {
                        builder.UseAzureAppConfiguration();
                    }
                    builder.AddJsonFile("/config/config.json", true);

                })
                .UseStartup<Startup>();
            });

    }
}
