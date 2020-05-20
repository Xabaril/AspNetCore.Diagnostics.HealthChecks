using HealthChecks.UI.Image.Configuration;
using HealthChecks.UI.Image.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthChecks.UI.Image
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)                
                .ConfigureLogging(config =>
                {
                    config.AddFilter(typeof(Program).Namespace, LogLevel.Information);
                })
                .ConfigureAppConfiguration(config =>
                {
                    if (AzureAppConfiguration.Enabled)
                    {
                        config.UseAzureAppConfiguration();
                    }
                })
                .UseStartup<Startup>();
        }
    }
}
