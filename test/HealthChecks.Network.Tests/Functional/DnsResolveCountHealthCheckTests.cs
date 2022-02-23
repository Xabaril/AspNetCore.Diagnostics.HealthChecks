using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HealthChecks.Network.Tests.Functional
{
    public class dns_resolve_host_count_should
    {
        private const string hostName = "google.com";
        private const string hostName2 = "microsoft.com";

        [Fact]
        public async Task be_healthy_when_the_configured_number_of_resolved_addresses_is_within_the_threshold()
        {
            var addresses = (await Dns.GetHostAddressesAsync(hostName)).Count();
            var addresses2 = (await Dns.GetHostAddressesAsync(hostName2)).Count();

            var webHostBuilder = new WebHostBuilder()
           .UseStartup<DefaultStartup>()
           .ConfigureServices(services =>
           {
               services
               .AddRouting()
               .AddHealthChecks()
                .AddDnsResolveHostCountHealthCheck(setup =>
                {
                    setup.AddHost(hostName, addresses, addresses);
                    setup.AddHost(hostName2, addresses2, addresses2);
                });
           })
           .Configure(app =>
           {
               app.UseRouting();
               app.UseEndpoints(config =>
               {
                   config.MapHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => true
                   });
               });

           });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateRequest("/health")
                .GetAsync();

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task be_unhealthy_when_the_configured_number_of_resolved_is_out_of_range()
        {
            var addresses = (await Dns.GetHostAddressesAsync(hostName)).Count();
            var addresses2 = (await Dns.GetHostAddressesAsync(hostName2)).Count();

            var webHostBuilder = new WebHostBuilder()
           .UseStartup<DefaultStartup>()
           .ConfigureServices(services =>
           {
               services
               .AddRouting()
               .AddHealthChecks()
                .AddDnsResolveHostCountHealthCheck(setup =>
                {
                    setup.AddHost(hostName, addresses + 1, addresses - 1);
                    setup.AddHost(hostName2, addresses2 + 1, addresses2 - 1);
                });
           })
           .Configure(app =>
           {
               app.UseRouting();
               app.UseEndpoints(config =>
               {
                   config.MapHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => true,
                       ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                   });
               });

           });

            var server = new TestServer(webHostBuilder);
            var response = await server.CreateClient().GetAsJson<UIHealthReport>("/health");
        }
    }
}
