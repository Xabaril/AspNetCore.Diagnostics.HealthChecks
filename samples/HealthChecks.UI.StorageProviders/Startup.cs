using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.UI.StorageProviders;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddRouting()
            .AddHealthChecks()
            .AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 200, tags: ["memory"])
            .AddCheck(name: "random", () => DateTime.UtcNow.Second % 2 == 0 ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy())
            .Services
            .AddHealthChecksUI(setup =>
            {
                setup.SetHeaderText("Storage providers demo");
                //Maximum history entries by endpoint
                setup.MaximumHistoryEntriesPerEndpoint(50);
                //One endpoint is configured in appsettings, let's add another one programatically
                setup.AddHealthCheckEndpoint("Endpoint2", "/random-health");
            })
            //Uncomment the options below to use different database providers
            //.AddSqlServerStorage("server=localhost;initial catalog=healthchecksui;user id=sa;password=Password12!");
            //.AddSqliteStorage("Data Source = healthchecks.db");
            .AddInMemoryStorage();
        //.AddPostgreSqlStorage("Host=localhost;Username=postgres;Password=Password12!;Database=healthchecksui");
        //.AddMySqlStorage("Host=localhost;User Id=root;Password=Password12!;Database=UI");
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app/*, IWebHostEnvironment env*/)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/memory-health", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("memory"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            endpoints.MapHealthChecks("/random-health", new HealthCheckOptions
            {
                Predicate = r => r.Name.Equals("random", StringComparison.InvariantCultureIgnoreCase),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            endpoints.MapHealthChecksUI();
        });
    }
}
