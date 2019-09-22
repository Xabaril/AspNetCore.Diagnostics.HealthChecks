using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.System
{
    public class process_healthcheck_should
    {
        [Fact]
        public async Task be_healthy_when_the_process_exists_and_is_running()
        {
            var webhostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddProcessHealthCheck("dotnet", p => !p.FirstOrDefault().HasExited);
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => r.Tags.Contains("process")
                    });
                });

            var server = new TestServer(webhostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
