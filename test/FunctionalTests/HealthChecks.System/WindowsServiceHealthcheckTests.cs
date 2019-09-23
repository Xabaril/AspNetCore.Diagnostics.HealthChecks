using FluentAssertions;
using FunctionalTests.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.ServiceProcess;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.System
{
    public class windows_service__healthcheck_should
    {
        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_healthy_when_the_service_is_running()
        {
            var webhostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddWindowsServiceHealthCheck("Windows Update", s => s.StartType  == ServiceStartMode.Manual);
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => true
                    });
                });

            var server = new TestServer(webhostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        
        [SkipOnPlatform(Platform.LINUX, Platform.OSX)]
        public async Task be_unhealthy_when_the_service_does_not_exist()
        {
            var webhostBuilder = new WebHostBuilder()
                .UseStartup<DefaultStartup>()
                .ConfigureServices(services =>
                {
                    services.AddHealthChecks()
                        .AddWindowsServiceHealthCheck("someservice", s => s.Status == ServiceControllerStatus.Running);
                })
                .Configure(app =>
                {
                    app.UseHealthChecks("/health", new HealthCheckOptions()
                    {
                        Predicate = r => true
                    });
                });

            var server = new TestServer(webhostBuilder);
            var response = await server.CreateRequest("/health").GetAsync();
            response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [SkipOnPlatform(Platform.WINDOWS)]
        public async Task throw_exception_when_registering_it_in_a_no_windows_system()
        {
            var webhostBuilder = new WebHostBuilder()
               .UseStartup<DefaultStartup>()
               .ConfigureServices(services =>
               {
                   services.AddHealthChecks()
                       .AddWindowsServiceHealthCheck("sv", s => s.Status == ServiceControllerStatus.Running);
               })
               .Configure(app =>
               {
                   app.UseHealthChecks("/health", new HealthCheckOptions()
                   {
                       Predicate = r => true
                   });
               });
            Assert.Throws<Exception>(() =>
            {
                var server = new TestServer(webhostBuilder);
            });
        }
    }
}
