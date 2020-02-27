using FluentAssertions;
using FunctionalTests.Base;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace FunctionalTests.HealthChecks.UI
{
    public class UI_concurrent_startup1_should
    {

        private async Task BaseConcurrencyTest()
        {
            var webHostBuilder = new WebHostBuilder()
              .UseStartup<DefaultStartup>()
              .ConfigureServices(services =>
              {
                  services.AddHealthChecks();
                  Action<Settings> setupSettings = settings =>
                  {
                      settings.AddHealthCheckEndpoint("Base", "/health");
                  };
                  services.AddHealthChecksUI(setupSettings: setupSettings);
              })
              .Configure(app => 
              {
                  app.UseHealthChecks("/health", new HealthCheckOptions
                  {
                      Predicate = _ => true,
                      ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                  });
                  app.UseHealthChecksUI();
              })
              .ConfigureAppConfiguration(conf =>
              {
                  conf.Sources.Clear();
                  conf.AddJsonFile("HealthChecks.UI/Configuration/appsettings.json", false);
              });

            var server = new TestServer(webHostBuilder);

            var response = await server.CreateRequest($"/health")
                .GetAsync();

            response.StatusCode
                    .Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public Task start_correctly_on_thread_1()
        {
            return BaseConcurrencyTest();
        }

        [Fact]
        public Task start_correctly_on_thread_2()
        {
            return BaseConcurrencyTest();
        }

        [Fact]
        public Task start_correctly_on_thread_3()
        {
            return BaseConcurrencyTest();
        }

        [Fact]
        public Task start_correctly_on_thread_4()
        {
            return BaseConcurrencyTest();
        }

        [Fact]
        public Task start_correctly_on_thread_5()
        {
            return BaseConcurrencyTest();
        }

        [Fact]
        public Task start_correctly_on_thread_6()
        {
            return BaseConcurrencyTest();
        }

        [Fact]
        public Task start_correctly_on_thread_7()
        {
            return BaseConcurrencyTest();
        }

        [Fact]
        public Task start_correctly_on_thread_8()
        {
            return BaseConcurrencyTest();
        }
    }

    public class UI_concurrent_startup2_should : UI_concurrent_startup1_should
    {
    }
}
